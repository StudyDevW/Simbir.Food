using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using ORM_Components;
using ORM_Components.DTO.ClientAPI;
using ORM_Components.DTO.RestaurantAPI;
using ORM_Components.Tables;
using ORM_Components.Tables.Helpers;
using ORM_Components.Validators.RestaurantFoodItemsValidators;
using RestaurantAPI.Model.Interface;
using RestaurantAPI.Utility;
using System.Collections.Generic;
using Telegram_Components.Interfaces;
using Telegram_Components.Services;

namespace RestaurantAPI.Model.Services
{
    public class RestaurantServices : IRestaurantServices
    {
        private readonly DataContext _dataContext;
        private readonly IMessageSender _tgmessageSender;
        private readonly IValidator<RestaurantUpdate_DTO> _restaurantUpdateValidator;

        public RestaurantServices(DataContext dataContext, IMessageSender messageSender
            , IValidator<RestaurantUpdate_DTO> restaurantUpdateValidator)
        {
            _dataContext = dataContext;
            _tgmessageSender = messageSender;
            _restaurantUpdateValidator = restaurantUpdateValidator;
        }

        private async Task<long> GetUserChatId(Guid clientId)
        {
            var chatId = await _dataContext.userTable
                .Where(x => x.Id == clientId)
                .Select(x => x.telegram_chat_id)
                .FirstOrDefaultAsync();
            if (chatId == 0) { throw new UserNotFoundException(clientId); }
            return chatId;
        }

        public async Task OrderRejections(Order_DTO order)
        {
            var orderRejections = await _dataContext.orderTable.FindAsync(order.id);

            if (orderRejections == null)
            {
                throw new OrderNotFoundException(order.id);
            }

            if (orderRejections.status == OrderStatus.Delivered)
            {
                throw new OrderAlreadyDelivered(order.id);
            }

            orderRejections.status = OrderStatus.Denied;

            await _dataContext.SaveChangesAsync();

            var userChatId = await GetUserChatId(order.client_id);
            var messageToClient = $"Ваш заказ с ID: {order.id} был отклонён.";
            await _tgmessageSender.Send(userChatId.ToString(), messageToClient);
        }

        public async Task DeleteAllRestaurant()
        {
            var restaurants = await _dataContext.restaurantTable.ToListAsync();
            _dataContext.restaurantTable.RemoveRange(restaurants);
            await _dataContext.SaveChangesAsync();
        }

        public async Task DeleteRestaurant(Guid restaurantId)
        {
            var restaurant = await _dataContext.restaurantTable
                .FirstOrDefaultAsync(x => x.Id == restaurantId)
                ?? throw new RestaurantNotFoundException(restaurantId);

            _dataContext.restaurantTable.Remove(restaurant);
            await _dataContext.SaveChangesAsync();
        }

        public async Task<List<Restaurants_DTO>> GetAllRestaurant()
        {
            var selectedRestaurants = await _dataContext.restaurantTable.ToListAsync();

            List<Restaurants_DTO> allRestaurants = new List<Restaurants_DTO>();

            foreach (var selectedRestaurant in selectedRestaurants)
            {
                var selectedReviews = await _dataContext.reviewTable.Where(c => c.restaurant_id == selectedRestaurant.Id).ToListAsync();

                var averageMarkCounts = 0f;

                var averageMark = 0f;

                if (selectedReviews.Count > 0)
                {
                    foreach (var review in selectedReviews)
                    {
                        averageMarkCounts += review.rating;
                    }

                    averageMark = averageMarkCounts / selectedReviews.Count;
                }

                Restaurants_DTO restaurantCompile = new Restaurants_DTO(
                    selectedRestaurant.Id,
                    selectedRestaurant.user_id,
                    selectedRestaurant.restaurantName,
                    selectedRestaurant.address,
                    selectedRestaurant.phone_number,
                    selectedRestaurant.status,
                    selectedRestaurant.description,
                    selectedRestaurant.imagePath,
                    selectedRestaurant.open_time,
                    selectedRestaurant.close_time,
                    averageMark
                );

                allRestaurants.Add(restaurantCompile);
            }

            return allRestaurants;
        }

        public async Task<Restaurants_DTO> GetRestaurant(Guid restaurantId)
        {
            var selectedRestaurant = await _dataContext.restaurantTable.Where(c => c.Id == restaurantId).FirstOrDefaultAsync();

            var selectedReviews = await _dataContext.reviewTable.Where(c => c.restaurant_id == restaurantId).ToListAsync();

            var averageMarkCounts = 0f;

            var averageMark = 0f;

            if (selectedReviews.Count > 0)
            {
                foreach (var review in selectedReviews)
                {
                    averageMarkCounts += review.rating;
                }

                averageMark = averageMarkCounts / selectedReviews.Count;
            }

            if (selectedRestaurant == null)
                throw new RestaurantNotFoundException(restaurantId);

            return new Restaurants_DTO
            (
                selectedRestaurant.Id,
                selectedRestaurant.user_id,
                selectedRestaurant.restaurantName,
                selectedRestaurant.address,
                selectedRestaurant.phone_number,
                selectedRestaurant.status,
                selectedRestaurant.description,
                selectedRestaurant.imagePath,
                selectedRestaurant.open_time,
                selectedRestaurant.close_time,
                averageMark
            );
        }

        public async Task UpdateRestaurant(Guid restaurantId, RestaurantUpdate_DTO restaurantsUpdate_DTO)
        {
            await _restaurantUpdateValidator.ValidateAndThrowAsync(restaurantsUpdate_DTO);

            var restaurant = await _dataContext.restaurantTable
               .FirstOrDefaultAsync(x => x.Id == restaurantId)
               ?? throw new RestaurantNotFoundException(restaurantId);

            var config = new TypeAdapterConfig();
            config.Default.IgnoreNullValues(true);
            restaurantsUpdate_DTO.Adapt(restaurant, config);
            await _dataContext.SaveChangesAsync();
        }

        public async Task<List<RestaurantMark_DTO>> GetRestaurantMark()
        {
            var restaurantWithMarks = await (
                from restaurant in _dataContext.restaurantTable
                let avg = _dataContext.reviewTable
                    .Where(r => r.restaurant_id == restaurant.Id)
                    .Select(r => (float?)r.rating)
                    .DefaultIfEmpty(0)
                    .Average()
                select new RestaurantMark_DTO(
                    restaurant.Id,
                    restaurant.restaurantName,
                    (int)avg)
            ).ToListAsync();

            return restaurantWithMarks;
        }

        public async Task SetReadyStatusForOrder(Guid orderId)
        {
            var order = await _dataContext.orderTable
                .FirstOrDefaultAsync(x => x.Id == orderId)
                ?? throw new OrderNotFoundException(orderId);

            order.status = OrderStatus.Ready;
            await _dataContext.SaveChangesAsync();

            var user = await _dataContext.userTable
                .FirstOrDefaultAsync(x => x.Id == order.client_id)
                ?? throw new UserNotFoundException(order.client_id);

            OrderStatusHistoryTable orderHistory = new OrderStatusHistoryTable()
            {
                order_id = orderId,
                status = OrderStatus.Ready,
                status_datetime = DateTime.UtcNow,
            };

            _dataContext.orderHistory.Add(orderHistory);
            await _dataContext.SaveChangesAsync();

            await _tgmessageSender.Send(user.telegram_chat_id.ToString(), "Ваш заказ ожидает курьера.");
            await SendMessageForEveryActiveCourier();
        }

        private async Task SendMessageForEveryActiveCourier()
        {
            var activeCouriersWithChatIds = await _dataContext.courierTable
            .Where(x => x.status == CourierStatus.IsActive)
            .Join(
                _dataContext.userTable,
                courier => courier.userId,
                user => user.Id,
                (courier, user) => user.telegram_chat_id
            )
            .ToListAsync();

            foreach (var telegram_chat_id in activeCouriersWithChatIds)
            {
                await _tgmessageSender.Send(telegram_chat_id.ToString(), "Доступен новый заказ к доставке!");
            }
        }

    }
}
