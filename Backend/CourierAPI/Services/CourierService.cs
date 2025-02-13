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
                FirstOrDefaultAsync(x => x.Id == orderLinkCourierDto.orderId);

            var courier = await _dataContext.courierTable
                .FirstOrDefaultAsync(x => x.Id == orderLinkCourierDto.courierId);

            if (courier == null) { throw new Exception("Курьер не найден."); }
            if (order == null) { throw new Exception("Заказ не найден."); }

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
                .FirstOrDefaultAsync(x => x.Id == orderId);

            if (order == null)
                throw new Exception("Заказ не найден.");

            if (order.status != expectedStatus)
                throw new Exception("Статус не соответствует требуемому.");

            order.status = newStatus;
            await _dataContext.SaveChangesAsync();
        }
    }
}
