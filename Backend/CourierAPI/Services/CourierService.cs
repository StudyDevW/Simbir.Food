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

namespace CourierAPI.Service
{
    public class CourierService : ICourierService
    {
        private readonly ILogger _logger;
        private readonly DataContext _dataContext;
        private readonly IMessageSender _tgmessage;
        private readonly IRabbitMQService _rabbitMQService;
        private readonly IValidator<CourierDtoForCreate> _courierCreateValidator;
        private readonly IValidator<CourierDtoForUpdate> _courierUpdateValidator;

        public CourierService(DataContext dataContext, IMessageSender tgmessage, 
            IRabbitMQService rabbitMQService,
            IValidator<CourierDtoForCreate> _courierCreateValidator, IValidator<CourierDtoForUpdate> _courierUpdateValidator) 
        { 
            _dataContext = dataContext;
            _tgmessage = tgmessage;
            _rabbitMQService = rabbitMQService;
            _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger("CourierAPI | database-sdk-logger");
        }

        public async Task<List<OrderForCourierDto>> GetOrders()
        {
            return await _dataContext.orderTable
                .Where(x => x.status == OrderStatus.Ready)
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

            order.courier_id = orderLinkCourierDto.courierId;
            await _dataContext.SaveChangesAsync();

            _logger.LogInformation($"Курьер с ID: {order.courier_id} был назначен на заказ: {order.Id}.");
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

            if (order.courier_id == Guid.Empty)
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
        }

        private async Task UpdateOrderStatus(Guid orderId, OrderStatus expectedStatus, OrderStatus newStatus)
        {
            var order = await _dataContext.orderTable
                .FirstOrDefaultAsync(x => x.Id == orderId)
                ?? throw new Exception("Заказ не найден.");             

            if (order.status != expectedStatus)
                throw new Exception("Статус не соответствует ожидаемому.");

            order.status = newStatus;

            _dataContext.orderStatusHistoryTables.Add(
                new OrderStatusHistoryTable
                {
                    Id = Guid.NewGuid(),
                    order_id = orderId,
                    status = newStatus,
                    status_datetime = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local)
                });

            await _dataContext.SaveChangesAsync();

            var user = await _dataContext.userTable
                .FirstOrDefaultAsync(x => x.Id == order.client_id);

            await _tgmessage.Send(user!.chatId,
                    $"Статус заказа был изменён с {expectedStatus} на {newStatus}");

            _logger.LogInformation($"Статус заказа с ID: {orderId} был изменён с {expectedStatus} на {newStatus}.");

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
