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


        public CourierService(DataContext dataContext, IMessageSender tgmessage,
            IRabbitMQService rabbitMQService, IMailSender mailSender,
            IValidator<CourierDtoForCreate> courierCreateValidator, IValidator<CourierDtoForUpdate> courierUpdateValidator)
        {
            _dataContext = dataContext;
            _tgmessage = tgmessage;
            _rabbitMQService = rabbitMQService;
            _mailSender = mailSender;
            _courierCreateValidator = courierCreateValidator;
            _courierUpdateValidator = courierUpdateValidator;
            _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger("CourierAPI | database-sdk-logger");
        }

        public async Task<List<OrderForCourierDto>> GetOrders(Guid courierId)
        {
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

            return await query
                .Select(x => new OrderForCourierDto(x.Id, x.client_id, x.status, x.order_date))
                .ToListAsync();
        }

        public async Task AcceptOrder(OrderLinkCourierDto orderLinkCourierDto)
        {
            var order = await _dataContext.orderTable.
                FirstOrDefaultAsync(x => x.Id == orderLinkCourierDto.orderId)
                ?? throw new Exception("Заказ не найден.");

            bool isCourierExist = await _dataContext.courierTable
                .AnyAsync(x => x.Id == orderLinkCourierDto.courierId);

            if (!isCourierExist)
                throw new Exception("Курьер не найден.");

            await CheckQuantityOfOrdersForCourier(orderLinkCourierDto.courierId);

            order.courier_id = orderLinkCourierDto.courierId;
            await _dataContext.SaveChangesAsync();

            _logger.LogInformation($"Курьер с ID: {order.courier_id} был назначен на заказ: {order.Id}.");
        }

        private async Task CheckQuantityOfOrdersForCourier(Guid courierId)
        {
            var ordersCount = await _dataContext.orderTable
                .Where(x => x.status == OrderStatus.WaitingForDelivery && x.courier_id == courierId)
                .CountAsync();

            if (ordersCount >= 3)
            {
                throw new Exception($"Курьер {courierId}, достиг лимита единовременных заказов: 3");
            }
        }

        public async Task TakeOrder(Guid orderId)
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
                ?? throw new Exception("Заказ не найден.");

            if (order.courier_id == null || order.courier_id == Guid.Empty)
            {
                throw new Exception("Курьер не назначен на заказ.");
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
                _rabbitMQService.SendMessage("mailsender", new EmailDto(orderId, user.email));
            }
        }

        private async Task UpdateOrderStatus(Guid orderId, OrderStatus expectedStatus, OrderStatus newStatus)
        {
            var order = await _dataContext.orderTable
                .FirstOrDefaultAsync(x => x.Id == orderId)
                ?? throw new Exception("Заказ не найден.");

            if (order.status != expectedStatus)
                throw new Exception("Статус не соответствует ожидаемому.");

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
                throw new Exception("Пользователь не найден.");

            var courier = courierDtoForCreate.Adapt<CourierTable>();
            _dataContext.Add(courier);
            await _dataContext.SaveChangesAsync();

            _logger.LogInformation($"Новый курьер был создан. ID: {courier.Id}.");
        }

        public async Task<CourierDto> GetAsync(Guid courierId)
        {
            return await _dataContext.courierTable
                .Where(x => x.Id == courierId)
                .Select(x => new CourierDto(x.Id, x.userId, x.car_number, x.status))
                .FirstOrDefaultAsync() ?? throw new Exception("Курьер не найден.");
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
                ?? throw new Exception("Курьер не найден.");

            courierDtoForUpdate.Adapt(courier);
            await _dataContext.SaveChangesAsync();

            _logger.LogInformation($"В курьера с ID: {courier.Id} были внесены изменения.");
        }

        public async Task DeleteAsync(Guid courierId)
        {
            var courier = await _dataContext.courierTable
                .FirstOrDefaultAsync(x => x.Id == courierId)
                ?? throw new Exception("Курьер не найден.");

            _dataContext.Remove(courier);
            await _dataContext.SaveChangesAsync();

            _logger.LogInformation($"Курьера с ID: {courier.Id} был удалён.");
        }

    }
}