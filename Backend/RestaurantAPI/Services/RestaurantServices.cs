using FluentValidation;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Middleware_Components.DTO.YandexDTO;
using Middleware_Components.Services;
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
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;

namespace RestaurantAPI.Model.Services
{
    public class RestaurantServices : IRestaurantServices
    {
        private readonly DataContext _dataContext;
        private readonly IMessageSender _tgmessageSender;
        private readonly IValidator<RestaurantUpdate_DTO> _restaurantUpdateValidator;
        private readonly IYandexIntegrationService _yandexIntegration;
        private readonly IJwtService _jwt;
        private readonly ICacheService _cache;

        public RestaurantServices(DataContext dataContext, IMessageSender messageSender,
            IValidator<RestaurantUpdate_DTO> restaurantUpdateValidator, IYandexIntegrationService yandexIntegration,
            IJwtService jwt, ICacheService cache)
        {
            _dataContext = dataContext;
            _tgmessageSender = messageSender;
            _restaurantUpdateValidator = restaurantUpdateValidator;
            _yandexIntegration = yandexIntegration;
            _jwt = jwt;
            _cache = cache;
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

        public async Task OrderRejections(Guid orderId)
        {
            var orderRejections = await _dataContext.orderTable.FindAsync(orderId);

            if (orderRejections == null)
            {
                throw new OrderNotFoundException(orderId);
            }

            if (orderRejections.status == OrderStatus.Delivered)
            {
                throw new OrderAlreadyDelivered(orderId);
            }

            orderRejections.status = OrderStatus.Denied;

            await _dataContext.SaveChangesAsync();

            Guid client_id = await _dataContext.orderTable
                .Where(x => x.Id == orderId)
                .Select(x => x.client_id)
                .FirstAsync();

            var userChatId = await GetUserChatId(client_id);
            var messageToClient = $"Ваш заказ с ID: {orderId} был отклонён.";
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

        private void YandexOptimizationInit(Guid userGUID, Guid restaurantId, YandexCoords clientCoords, YandexCoords restaurantCoords)
        {
            YandexInfoCached yandexCached = new YandexInfoCached()
            {
                coordsClient = clientCoords,
                coordsRestaurant = restaurantCoords
            };

            _cache.WriteKeyInStorage(userGUID, $"cached_yandex_address_info_{restaurantId}", yandexCached, DateTime.UtcNow.AddDays(1));
        }

        private bool ChangedClientAddress(Guid userGUID, Guid restaurantId)
        {
            var selectedClient = _dataContext.userTable.Where(c => c.Id == userGUID).FirstOrDefault();

            var currentCached = _cache.GetKeyFromStorage<YandexInfoCached>(userGUID, $"cached_yandex_address_info_{restaurantId}");

            if (currentCached.coordsClient.address != selectedClient!.address)
                return true;

            return false;
        }

        private bool ChangedRestaurantAddress(Guid userGUID, Guid restaurantId)
        {
            var selectedRestaurant = _dataContext.restaurantTable.Where(c => c.Id == restaurantId).FirstOrDefault();

            var currentCached = _cache.GetKeyFromStorage<YandexInfoCached>(userGUID, $"cached_yandex_address_info_{restaurantId}");

            if (currentCached.coordsRestaurant.address != selectedRestaurant!.address)
                return true;

            return false;
        }

        public async Task<List<Restaurants_DTO>> GetAllRestaurant(string bearerKey, string? search)
        {
            var validation = await _jwt.AccessTokenValidation(bearerKey);

            if (validation.TokenHasError())
                throw new Exception("token_invalid");

            var selectedRestaurants = 
                search == null ? 
                await _dataContext.restaurantTable.Where(x => x.status == RestaurantStatus.Verified).ToListAsync() : 
                await _dataContext.restaurantTable.Where(x => x.status == RestaurantStatus.Verified && (x.restaurantName.ToLower().Contains(search.ToLower()) || x.restaurantName.ToUpper().Contains(search.ToUpper())))
                .ToListAsync();

            var selectedClient = await _dataContext.userTable.Where(c => c.Id == validation.token_success!.Id).FirstOrDefaultAsync();

            if (selectedClient == null)
                throw new Exception("user_not_found");

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

                //Если в кэше нет информации о координатах, то достаем их и записываем
                if (!_cache.CheckExistKeysStorage<YandexInfoCached>(selectedClient.Id, $"cached_yandex_address_info_{selectedRestaurant.Id}"))
                {
                    var coordinatesRestaurant = await _yandexIntegration.GetCoordinatesFromAddress(selectedRestaurant.address);

                    var coordinatesClient = await _yandexIntegration.GetCoordinatesFromAddress(selectedClient.address!);

                    YandexOptimizationInit(selectedClient.Id, selectedRestaurant.Id, coordinatesClient!, coordinatesRestaurant!);
                }
                else //Если в кэше записаны данные о координатах, то проверяем сменился ли адрес
                {
                    //Две проверки были сделаны для оптимизации запросов на ключ яндекса
                    //Если бы была одна проверка, то на изменение одного только адреса клиента, мы бы тратили два запроса
                    if (ChangedClientAddress(selectedClient.Id, selectedRestaurant.Id))
                    {
                        var preCachedInfoCoords = _cache.GetKeyFromStorage<YandexInfoCached>(selectedClient.Id, $"cached_yandex_address_info_{selectedRestaurant.Id}");

                        var coordinatesRestaurant = preCachedInfoCoords.coordsRestaurant;

                        var coordinatesClient = await _yandexIntegration.GetCoordinatesFromAddress(selectedClient.address!);

                        YandexOptimizationInit(selectedClient.Id, selectedRestaurant.Id, coordinatesClient!, coordinatesRestaurant!);
                    }

                    if (ChangedRestaurantAddress(selectedClient.Id, selectedRestaurant.Id))
                    {
                        var preCachedInfoCoords = _cache.GetKeyFromStorage<YandexInfoCached>(selectedClient.Id, $"cached_yandex_address_info_{selectedRestaurant.Id}");

                        var coordinatesRestaurant = await _yandexIntegration.GetCoordinatesFromAddress(selectedRestaurant.address);

                        var coordinatesClient = preCachedInfoCoords.coordsClient;

                        YandexOptimizationInit(selectedClient.Id, selectedRestaurant.Id, coordinatesClient!, coordinatesRestaurant!);
                    }
                }

                var cachedInfoCoords = _cache.GetKeyFromStorage<YandexInfoCached>(selectedClient.Id, $"cached_yandex_address_info_{selectedRestaurant.Id}");

                var travelInformation = GetDistanceInfo(cachedInfoCoords.coordsClient, cachedInfoCoords.coordsRestaurant);

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
                    averageMark,
                    travelInformation
                );

                allRestaurants.Add(restaurantCompile);
            }
            Console.WriteLine("Посмотри " + allRestaurants.Count);
            return allRestaurants.Where(c => c.travelInfo.distanceKM <= 4.0).ToList();
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

            var coordinates = await _yandexIntegration.GetCoordinatesFromAddress(selectedRestaurant.address);

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
                averageMark,
                null
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

        private static double CalculateHaversineDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double EarthRadiusKm = 6371.0;
            double dLat = DegreesToRadians(lat2 - lat1);
            double dLon = DegreesToRadians(lon2 - lon1);

            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return EarthRadiusKm * c;
        }

        private static double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }

        private YandexDistance GetDistanceInfo(YandexCoords coordsFrom, YandexCoords coordsTo)
        {
            double distance = CalculateHaversineDistance(coordsFrom.lat, coordsFrom.lon, coordsTo.lat, coordsTo.lon);
            double timeHours = distance / 15;

            return new YandexDistance() { distanceKM = distance, timeHours = timeHours };
        }
    }
}
