using Mapster;
using Microsoft.EntityFrameworkCore;
using ORM_Components;
using ORM_Components.DTO.ClientAPI;
using ORM_Components.DTO.RestaurantAPI;
using ORM_Components.Tables;
using ORM_Components.Tables.Helpers;
using RestaurantAPI.Model.Interface;
using Telegram_Components.Interfaces;
using Telegram_Components.Services;

namespace RestaurantAPI.Model.Services
{
    public class RestaurantServices : IRestaurantServices
    {
        private readonly DataContext _dataContext;
        private readonly IMessageSender _tgmessageSender;

        public RestaurantServices(DataContext dataContext, IMessageSender messageSender)
        {
            _dataContext = dataContext;
            _tgmessageSender = messageSender;
        }

        private long GetUserChatId(Guid? clientId)
        {
            var finded = _dataContext.userTable.Where(c => c.Id == clientId).FirstOrDefault();
            return finded.telegram_chat_id;
        }
        public async Task OrderRejections(Order_DTO order)
        {
            var orderRejections = await _dataContext.orderTable.FindAsync(order.id);

            if (orderRejections == null)
            {
                throw new Exception($"Заказ с идентификатором: {order.id} не существует.");
            }

            orderRejections.status = OrderStatus.Denied;

            await _dataContext.SaveChangesAsync();

            var userChatId = GetUserChatId(order.client_id);
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
                ?? throw new Exception("Ресторан не найден.");

            _dataContext.restaurantTable.Remove(restaurant);
            await _dataContext.SaveChangesAsync();
        }
        public async Task<List<Restaurants_DTO>> GetAllRestaurant()
        {
            return await _dataContext.restaurantTable
                .Select(x => new Restaurants_DTO(
                    x.Id, x.user_id, x.restaurantName,
                    x.address, x.phone_number, x.status,
                    x.description, x.imagePath, x.open_time,
                    x.close_time))
                .ToListAsync();
        }
        public async Task<Restaurants_DTO> GetRestaurant(Guid restaurantId)
        {
            return await _dataContext.restaurantTable
                .Where(x => x.Id == restaurantId)
                .Select(x => new Restaurants_DTO(
                    x.Id, x.user_id, x.restaurantName,
                    x.address, x.phone_number, x.status,
                    x.description, x.imagePath, x.open_time,
                    x.close_time))
                .FirstOrDefaultAsync() ?? throw new Exception("Ресторан не найден.");
        }
        public async Task UpdateRestaurant(Guid restaurantId, RestaurantUpdate_DTO restaurantsUpdate_DTO)
        {
            var restaurant = await _dataContext.restaurantTable
               .FirstOrDefaultAsync(x => x.Id == restaurantId)
               ?? throw new Exception("Ресторан не найден.");

            restaurantsUpdate_DTO.Adapt(restaurant);
            await _dataContext.SaveChangesAsync();
        }
        public async Task<List<RestaurantMark_DTO>> GetRestaurantMark()
        {
            var restaurantWithMarks = await _dataContext.restaurantTable
            .GroupJoin(
                _dataContext.reviewTable,
                restaurant => restaurant.Id,
                review => review.restaurant_id,
                (restaurant, reviews) => new
                {
                    restaurant.Id,
                    restaurant.restaurantName,
                    AverageMark = reviews.Any() ? reviews.Average(r => r.rating) : 0
                })
            .ToListAsync();

            return restaurantWithMarks.Select(r =>
                new RestaurantMark_DTO(r.Id, r.restaurantName, (int)r.AverageMark)
            ).ToList();
        }
        public async Task SetReadyStatusForOrder(Guid orderId)
        {
            var order = await _dataContext.orderTable
                .FirstOrDefaultAsync(x => x.Id == orderId)
                ?? throw new Exception("Заказ не найден.");

            order.status = OrderStatus.Ready;
            await _dataContext.SaveChangesAsync();

            var user = await _dataContext.userTable
                .FirstOrDefaultAsync(x => x.Id == order.client_id)
                ?? throw new Exception("Пользователь не найден.");

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
