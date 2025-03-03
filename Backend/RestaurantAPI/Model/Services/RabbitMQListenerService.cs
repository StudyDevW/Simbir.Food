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
        private readonly RabbitMQService _rabbitMQService;
        private readonly IMessageSender _messageSender;
        private readonly DataContext _dbcontext;

        public RabbitMQListenerService(RabbitMQService rabbitMQService, IMessageSender messageSender, DataContext dbcontext)
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
                    // Реализация обновление заказа
                    await OrderUpdate(order_DTO);
                    // Реализация готовности заказа
                    await  OrderFinished(order_DTO);
                    //// Реализация отклонения заказа
                    //await OrderRejections(order_DTO);
                });
                _rabbitMQService.StartListening<Order_DTO>("restaurant_to_courier", async order_DTO =>
                {
                    // Реализация отправки информации по заказу курьеру
                    await SendingOrderInformationCourier(order_DTO);

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
        private async Task NotifyOrderFinishedClient(Order_DTO order)
        {
            var userChatId = GetUserChatId(order.client_id);
            var message = $"Ваш заказ с ID: {order.id} приготовлен, мы ищем курбера который доставит вам заказ.";
            await _messageSender.Send(userChatId.ToString(), message);
        }
        private async Task NotifyOrderFinishedCourier(Order_DTO order)
        {
            //if (order.courier_id == null)
            //    throw new Exception("");

            //var courierChatId = await GetCourierChatIdForOrder(order.courier_id);
            //var message = $"Теперь вам доступен новый заказ для приготовления и доставки";
            //await _messageSender.Send(courierChatId.ToString(), message);
        }
        private async Task NotifyOrderCanceled(Order_DTO order)
        {
            var userChatId = GetUserChatId(order.client_id);
            var message = $"Ваш заказ с ID: {order.id} был отменен.";
            await _messageSender.Send(userChatId.ToString(), message);
        }

        private long GetUserChatId(Guid clientId)
        {
            var finded = _dbcontext.userTable.Where(c => c.Id == clientId).FirstOrDefault();
            return finded.telegram_chat_id;
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

            var userChatId = GetUserChatId(order.client_id);
            await NotifyOrderStarted(order);
            await SendingOrderInformationCourier(order);
        }

        private async Task OrderRejections(Order_DTO order)
        {
            //Поиск в бд
            var orderRejections = await _dbcontext.orderTable.FindAsync(order.id);

            if (orderRejections == null)
            {
                // Если заказ не найден, можно выбросить исключение или логировать ошибку
                throw new Exception($"Заказ с идентификатором: {order.id} не существует.");
            }

            orderRejections.status = OrderStatus.Denied;

            await _dbcontext.SaveChangesAsync();

            var userChatId = GetUserChatId(order.client_id);
            var messageToClient = $"Ваш заказ с ID: {order.id} был отклонён.";
            await _messageSender.Send(userChatId.ToString(), messageToClient);

            await SendingOrderInformationCourier(order);
        }

        private async Task OrderFinished(Order_DTO order)
        {
            var orderFinished = await _dbcontext.orderTable.FindAsync(order.id);
            
            if(orderFinished == null)
            {
                throw new Exception($"Заказ с идентификатором: {order.id} не существует.");
            }

            orderFinished.status = OrderStatus.Ready;
            await _dbcontext.SaveChangesAsync();
            await NotifyOrderFinishedClient(order);
        }

        private async Task OrderUpdate(Order_DTO order)
        {
            var orderDetails = await _dbcontext.orderTable.FindAsync(order.id);

            if (orderDetails == null)
            {
                throw new Exception($"Заказ с ID: {order.id} не удалось получить из ClientAPI.");
            }

            var orderEntity = await _dbcontext.orderTable.FindAsync(order.id);
            if (orderEntity == null)
            {
                throw new Exception($"Заказ с ID: {order.id} не существует в базе данных.");
            }

            orderEntity.status = orderDetails.status;
            await _dbcontext.SaveChangesAsync();

            var userChatId = GetUserChatId(order.client_id);
            var messageToClient = $"Ваш заказ с ID: {order.id} был обновлён.";
            await _messageSender.Send(userChatId.ToString(), messageToClient);
        }
        
        private async Task<long?> GetCourierChatIdForOrder(Guid orderId)
        {
            var order = await _dbcontext.orderTable.FindAsync(orderId);

            if (order == null)
            {
                throw new Exception($"Заказ с ID: {orderId} не найден в базе данных.");
            }
            if (order.courier_id.HasValue)
            {
                var courier = await _dbcontext.userTable.FindAsync(order.courier_id.Value);

                if (courier == null)
                {
                    throw new Exception($"Курьер с ID: {order.courier_id} не найден.");
                }
                return courier.telegram_chat_id;
            }

            return null;
        }
        private async Task SendingOrderInformationCourier(Order_DTO order)
        {
            var courierChatId = await GetCourierChatIdForOrder(order.id);

            if (courierChatId == null)
            {
                throw new Exception($"Курьер для заказа с ID: {order.id} не найден.");
            }
            await NotifyOrderFinishedCourier(order);

            var orderDetailsMessage = await GenerateOrderDetailsMessage(order);
            await _messageSender.Send(courierChatId.Value.ToString(), orderDetailsMessage);
            
        }

        private async Task SendMessageForEveryActiveCourier()
        {
            var activeCouriersWithChatIds = await _dbcontext.courierTable
            .Where(x => x.status == CourierStatus.IsActive)
            .Join(
                _dbcontext.userTable,
                courier => courier.userId,
                user => user.Id,
                (courier, user) => user.telegram_chat_id
            )
            .ToListAsync();

            foreach (var chatId in activeCouriersWithChatIds)
            {
                await _messageSender.Send(chatId.ToString(), "Доступен новый заказ к доставке!");
            }
        }
        private async Task<string> GenerateOrderDetailsMessage(Order_DTO order)
        {
            var restaurant = await _dbcontext.restaurantTable.FindAsync(order.restaurant_id);
            if (restaurant == null)
            {
                throw new Exception($"Ресторан с ID: {order.restaurant_id} не найден.");
            }

            var orderItems = await _dbcontext.orderItemsTable
                .Where(item => item.order_id == order.id)
                .ToListAsync();

            //var items = string.Join(", ", orderItems.Select(item => $"{item.quantity}x {item.Id}"));

            return $"Заказ ID: {order.id}\n" +
                   $"Ресторан: {restaurant.restaurantName}\n" +
                   $"Клиент: {GetClientName(order.client_id)}\n" + 
                   $"Адрес: {GetClientAddress(order.client_id)}\n" +
                   //$"Товары: {items}\n" +
                   $"Итого: {order.total_price} руб.\n" +
                   $"Дата заказа: {order.order_date.ToString("g")}";
        }

        private string GetClientName(Guid clientId)
        {
            var client = _dbcontext.userTable.Find(clientId);
            return client?.username ?? "Неизвестный клиент";
        }

        private string GetClientAddress(Guid clientId)
        {
            var client = _dbcontext.userTable.Find(clientId);
            return client?.address ?? "Адрес не указан";
        }
    }
}
