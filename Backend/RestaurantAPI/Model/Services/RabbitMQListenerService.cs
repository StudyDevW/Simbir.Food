using Mapster;
using Microsoft.EntityFrameworkCore;
using Middleware_Components.Broker;
using ORM_Components;
using ORM_Components.DTO.ClientAPI;
using ORM_Components.DTO.CourierAPI;
using ORM_Components.DTO.RestaurantAPI;
using ORM_Components.Tables;
using ORM_Components.Tables.Helpers;
using StackExchange.Redis;
using Telegram.Bot.Types;
using Telegram_Components.Interfaces;
using Telegram_Components.Services;

namespace RestaurantAPI.Services
{
    public class RabbitMQListenerService : BackgroundService
    {
        private readonly IRabbitMQService _rabbitMQService;
        private readonly IMessageSender _messageSender;
        private readonly DataContext _dbcontext;

        public RabbitMQListenerService(IRabbitMQService rabbitMQService, IMessageSender messageSender, DataContext dbcontext)
        {
            _rabbitMQService = rabbitMQService;
            _messageSender = messageSender;
            _dbcontext = dbcontext;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Task.Run(() =>
            {
                _rabbitMQService.StartListening<Order_DTO>("client_to_restaurant", async order_DTO =>
                {
                    // Реализация принятие заказа
                    await AcceptingAnOrder(order_DTO);
                });

            }, stoppingToken);

            return Task.CompletedTask;
        }

        private async Task NotifyOrderStarted(Order_DTO order)
        {
            var userChatId = GetUserChatId(order.client_id);
            var message = $"Ваш заказ с ID: {order.id} начал готовиться.";
            await _messageSender.Send(userChatId.ToString(), message);
        }

        private long GetUserChatId(Guid? clientId)
        {
            var finded = _dbcontext.userTable.Where(c => c.Id == clientId).FirstOrDefault();
            return finded!.telegram_chat_id;
        }
 
        private async Task AcceptingAnOrder(Order_DTO order)
        {
            var orderAccepting = await _dbcontext.orderTable.FindAsync(order.id);

            if (orderAccepting == null)
            {
                throw new Exception($"Заказ с идентификатором: {order.id} не существует.");
            }

            orderAccepting.status = OrderStatus.Accepted; 
            await _dbcontext.SaveChangesAsync();

            await NotifyOrderStarted(order);
        }
    }
}
