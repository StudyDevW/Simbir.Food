using CourierAPI.Contracts;
using ORM_Components;
using ORM_Components.DTO.CourierAPI;
using Mapster;
using Microsoft.EntityFrameworkCore;
using ORM_Components.Tables.Helpers;
using StackExchange.Redis;
using System.Diagnostics.Metrics;
using ORM_Components.Tables;

namespace CourierAPI.Service
{
    public class CourierService : ICourierService
    {
        private readonly ILogger _logger;
        private readonly DataContext _dataContext;

        public CourierService(DataContext dataContext) 
        { 
            _dataContext = dataContext;
            _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger("CourierAPI | database-sdk-logger");
        }

        public async Task<List<OrderForCourierDto>> GetOrders()
        {
            var orders = await _dataContext.orderTable
                .Where(x => x.status == OrderStatus.Ready)
                .ProjectToType<OrderForCourierDto>()
                .ToListAsync();
            return orders;
        }

        public async Task AcceptOrder(OrderLinkCourierDto orderLinkCourierDto)
        {
            var order = await _dataContext.orderTable.
                FirstOrDefaultAsync(x => x.Id == orderLinkCourierDto.orderId)
                ?? throw new Exception("Заказ не найден."); ;

            var courier = await _dataContext.courierTable
                .FirstOrDefaultAsync(x => x.Id == orderLinkCourierDto.courierId)
                ?? throw new Exception("Курьер не найден."); ;

            order.courier_id = orderLinkCourierDto.courierId;
            await _dataContext.SaveChangesAsync();
            return;
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
        }

        private async Task UpdateOrderStatus(Guid orderId, OrderStatus expectedStatus, OrderStatus newStatus)
        {
            var order = await _dataContext.orderTable
                .FirstOrDefaultAsync(x => x.Id == orderId)
                ?? throw new Exception("Заказ не найден.");             

            if (order.status != expectedStatus)
                throw new Exception("Статус не соответствует требуемому.");

            order.status = newStatus;
            await _dataContext.SaveChangesAsync();
        }

        public async Task CreateAsync(CourierDtoForCreate courierDtoForCreate)
        {
            var user = await _dataContext.userTable
                .FirstOrDefaultAsync(x => x.Id == courierDtoForCreate.userId);

            if (user == null)
                throw new Exception("Пользователь не найден.");

            var courier = courierDtoForCreate.Adapt<CourierTable>();
            _dataContext.Add(courier);
            await _dataContext.SaveChangesAsync();
        }

        public async Task<CourierDto> GetAsync(Guid courierId)
        {
            return await _dataContext.courierTable
                .Where(x => x.Id == courierId)
                .ProjectToType<CourierDto>()
                .FirstOrDefaultAsync() ?? throw new Exception("Курьер не найден.");
        }

        public async Task<List<CourierDto>> GetAllAsync()
        {
            return await _dataContext.courierTable
                .ProjectToType<CourierDto>()
                .ToListAsync();
        }

        public async Task UpdateAsync(CourierDtoForUpdate courierDtoForUpdate)
        {
            var courier = await _dataContext.courierTable
                .FirstOrDefaultAsync(x => x.Id == courierDtoForUpdate.Id)
                ?? throw new Exception("Курьер не найден.");

            courierDtoForUpdate.Adapt(courier);

            await _dataContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid courierId)
        {
            var courier = _dataContext.courierTable
                .FirstOrDefaultAsync(x => x.Id == courierId)
                ?? throw new Exception("Курьер не найден.");

            _dataContext.Remove(courier);
            await _dataContext.SaveChangesAsync();
        }
    }
}
