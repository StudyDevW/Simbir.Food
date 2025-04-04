using CourierAPI.Contracts;
using ORM_Components;
using ORM_Components.DTO.CourierAPI;
using Mapster;
using Microsoft.EntityFrameworkCore;
using ORM_Components.Tables.Helpers;
using StackExchange.Redis;
using System.Diagnostics.Metrics;
using ORM_Components.Tables;
using Telegram_Components.Interfaces;
using System.Linq;
using Middleware_Components.Broker;
using ORM_Components.DTO.RestaurantAPI;
using FluentValidation;
using ORM_Components.Interfaces;
using ORM_Components.DTO.MailDtos;
using System.Security.Claims;
using RestaurantAPI.Utility;

namespace CourierAPI.Service
{
    public class CourierService : ICourierService
    {
        private readonly ILogger _logger;
        private readonly DataContext _dataContext;
        private readonly IMessageSender _tgmessage;
        private readonly IRabbitMQService _rabbitMQService;
        private readonly IMailSender _mailSender;
        private readonly IValidator<CourierDtoForCreate> _courierCreateValidator;
        private readonly IValidator<CourierDtoForUpdate> _courierUpdateValidator;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CourierService(DataContext dataContext, IMessageSender tgmessage,
            IRabbitMQService rabbitMQService, IMailSender mailSender,
            IValidator<CourierDtoForCreate> courierCreateValidator, IValidator<CourierDtoForUpdate> courierUpdateValidator,
            IHttpContextAccessor httpContextAccessor)
        {
            _dataContext = dataContext;
            _tgmessage = tgmessage;
            _rabbitMQService = rabbitMQService;
            _mailSender = mailSender;
            _courierCreateValidator = courierCreateValidator;
            _courierUpdateValidator = courierUpdateValidator;
            _httpContextAccessor = httpContextAccessor;
            _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger("CourierAPI | database-sdk-logger");
        }

        private async Task<Guid> GetCourierId()
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirst("Id")?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("Пользователь не авторизован");
            }

            Guid guidUserId = Guid.Parse(userId);

            return await _dataContext.courierTable
                .Where(x => x.userId == guidUserId)
                .Select(x => x.Id)
                .FirstOrDefaultAsync();
        }

        public async Task<List<OrderForCourierDto>> GetOrders()
        {
            Guid courierId = await GetCourierId();

            var restaurantId = await _dataContext.orderTable
                .Where(x => x.courier_id == courierId && x.status == OrderStatus.WaitingForDelivery)
                .OrderByDescending(x => x.order_date)
                .Select(x => x.restaurant_id)
                .FirstOrDefaultAsync();

            var query = _dataContext.orderTable
                .Where(x => x.status == OrderStatus.Ready);

            if (restaurantId != Guid.Empty)
            {
                query = query
                    .Where(x => x.restaurant_id == restaurantId);
            }

            List<OrderForCourierDtoShort> orderData = await query
                .Select(x => new OrderForCourierDtoShort(x.Id, x.restaurant_id, x.client_id, x.status, x.order_date, x.client_address))
                .ToListAsync();

            List<OrderForCourierDto> data = new();

            foreach (var order in orderData)
            {
                var restaurantInfo = await _dataContext.restaurantTable
                    .Where(x => x.Id == order.restaurantId)
                    .Select(x => new
                    {
                        x.Id,
                        x.restaurantName,
                        x.address,
                        x.phone_number
                    })
                    .FirstOrDefaultAsync();

                var clientInfo = await _dataContext.userTable
                    .Where(x => x.Id == order.clientId)
                    .Select(x => new { x.first_name, x.last_name, x.address, x.photo_url})
                    .FirstOrDefaultAsync();

                data.Add(new OrderForCourierDto(order.orderId, restaurantInfo.Id, 
                    restaurantInfo.restaurantName, restaurantInfo.address,
                    restaurantInfo.phone_number, order.client_address, 
                    clientInfo.photo_url, clientInfo.first_name, 
                    clientInfo.last_name, order.orderDate,
                    order.status));
            }
            return data;
        }

        public async Task AcceptOrder(Guid orderId)
        {
            Guid courierId = await GetCourierId();

            var order = await _dataContext.orderTable.
                FirstOrDefaultAsync(x => x.Id == orderId)
                ?? throw new OrderNotFoundException(orderId);

            bool isCourierExist = await _dataContext.courierTable
                .AnyAsync(x => x.Id == courierId);

            if (!isCourierExist)
                throw new CourierNotFoundException(courierId);

            await CheckQuantityOfOrdersForCourier(courierId);

            order.courier_id = courierId;
            await _dataContext.SaveChangesAsync();

            await TakeOrder(order.Id);

            _logger.LogInformation($"Курьер с ID: {order.courier_id} был назначен на заказ: {order.Id}.");
        }

        private async Task CheckQuantityOfOrdersForCourier(Guid courierId)
        {
            var ordersCount = await _dataContext.orderTable
                .Where(x => x.status == OrderStatus.WaitingForDelivery && x.courier_id == courierId)
                .CountAsync();

            if (ordersCount >= 3)
            {
                throw new OrderLimitForCourierException(courierId);
            }
        }

        private async Task TakeOrder(Guid orderId)
        {
            await UpdateOrderStatus(orderId, OrderStatus.Ready, OrderStatus.WaitingForDelivery);
        }

        public async Task CourierOnPlace(Guid orderId)
        {
            await UpdateOrderStatus(orderId, OrderStatus.WaitingForDelivery, OrderStatus.CourierOnPlace);
        }

        public async Task OrderDelivered(Guid orderId)
        {
            await UpdateOrderStatus(orderId, OrderStatus.CourierOnPlace, OrderStatus.Delivered);

            var order = await _dataContext.orderTable
                .FirstOrDefaultAsync(x => x.Id == orderId)
                ?? throw new OrderNotFoundException(orderId);

            if (order.courier_id == null || order.courier_id == Guid.Empty)
            {
                throw new OrderDeliveryException(orderId);
            }

            OrderIdsDto orderDto = new OrderIdsDto
            (
                order.Id,
                order.client_id,
                order.restaurant_id,
                order.courier_id!.Value
            );
            _rabbitMQService.SendMessage("order_review_queue", orderDto);

            var user = await _dataContext.userTable
                .FirstOrDefaultAsync(x => x.Id == order.client_id);

            if (user != null && user.email != null)
            {
                EmailDto emailDto = new EmailDto(orderId, user.email);
                _rabbitMQService.SendMessage("courier_to_orm", emailDto);
            }
        }

        private async Task UpdateOrderStatus(Guid orderId, OrderStatus expectedStatus, OrderStatus newStatus)
        {
            var order = await _dataContext.orderTable
                .FirstOrDefaultAsync(x => x.Id == orderId)
                ?? throw new OrderNotFoundException(orderId);

            if (order.status != expectedStatus)
                throw new OrderStatusException(orderId);

            order.status = newStatus;

            _dataContext.orderHistory.Add(
                new OrderStatusHistoryTable
                {
                    Id = Guid.NewGuid(),
                    order_id = orderId,
                    status = newStatus,
                    status_datetime = DateTime.UtcNow
                });

            await _dataContext.SaveChangesAsync();

            var user = await _dataContext.userTable
                .FirstOrDefaultAsync(x => x.Id == order.client_id);

            switch(newStatus)
            {
                case OrderStatus.WaitingForDelivery:
                    await SendTgMessageForClient(user!.telegram_chat_id.ToString(),
                        $"Ваш заказ с номером {order.Id} принят в доставку. Курьер уже в пути!");
                    break;

                case OrderStatus.CourierOnPlace:
                    await SendTgMessageForClient(user!.telegram_chat_id.ToString(),
                        $"Ваш заказ с номером {order.Id} доставлен к месту назначения.");
                    break;

                case OrderStatus.Delivered:
                    await SendTgMessageForClient(user!.telegram_chat_id.ToString(),
                        $"Ваш заказ с номером {order.Id} успешно доставлен. Приятного аппетита!");
                    break;
            }

            _logger.LogInformation($"Статус заказа с ID: {orderId} был изменён с {expectedStatus} на {newStatus}.");

        }

        private async Task SendTgMessageForClient(string chatId, string message)
        {
            await _tgmessage.Send(chatId, message);
        }

        public async Task CreateAsync(CourierDtoForCreate courierDtoForCreate)
        {
            await _courierCreateValidator.ValidateAsync(courierDtoForCreate);

            var isUserExist = await _dataContext.userTable
                .AnyAsync(x => x.Id == courierDtoForCreate.userId);

            if (!isUserExist)
                throw new UserNotFoundException(courierDtoForCreate.userId);

            var courier = courierDtoForCreate.Adapt<CourierTable>();
            _dataContext.Add(courier);
            await _dataContext.SaveChangesAsync();

            _logger.LogInformation($"Новый курьер был создан. ID: {courier.Id}.");
        }

        public async Task<CourierDto> GetAsync()
        {
            var courierId = await GetCourierId();

            return await _dataContext.courierTable
                .Where(x => x.Id == courierId)
                .Select(x => new CourierDto(x.Id, x.userId, x.car_number, x.status))
                .FirstOrDefaultAsync() ?? throw new CourierNotFoundException(courierId);
        }

        public async Task<List<CourierDto>> GetAllAsync()
        {
            var couriers = await _dataContext.courierTable
                .Select(x => new CourierDto(x.Id, x.userId, x.car_number, x.status))
                .ToListAsync();
            return couriers;
        }

        public async Task UpdateAsync(CourierDtoForUpdate courierDtoForUpdate)
        {
            await _courierUpdateValidator.ValidateAsync(courierDtoForUpdate);

            var courier = await _dataContext.courierTable
                .FirstOrDefaultAsync(x => x.Id == courierDtoForUpdate.Id)
                ?? throw new CourierNotFoundException(courierDtoForUpdate.Id);

            courierDtoForUpdate.Adapt(courier);
            await _dataContext.SaveChangesAsync();

            _logger.LogInformation($"В курьера с ID: {courier.Id} были внесены изменения.");
        }

        public async Task DeleteAsync(Guid courierId)
        {
            var courier = await _dataContext.courierTable
                .FirstOrDefaultAsync(x => x.Id == courierId)
                ?? throw new CourierNotFoundException(courierId);

            _dataContext.Remove(courier);
            await _dataContext.SaveChangesAsync();

            _logger.LogInformation($"Курьера с ID: {courier.Id} был удалён.");
        }
    }
}