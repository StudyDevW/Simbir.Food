using ClientAPI.Interfaces;
using Middleware_Components.Broker;
using ORM_Components.DTO.CourierAPI;
using ORM_Components.DTO.PaymentAPI;
using ORM_Components.Tables;
using Telegram.Bot.Types;
using Telegram_Components.Interfaces;

namespace ClientAPI.Services
{
    public class RabbitMQListenerService : BackgroundService
    {
        private readonly IRabbitMQService _rabbitMQService;
        private readonly IMessageSender _tgmessage;
        private readonly IDatabaseService _database;

        public RabbitMQListenerService(IDatabaseService database, IMessageSender tgmessage, IRabbitMQService rabbitMQService)
        {
            _rabbitMQService = rabbitMQService;
            _tgmessage = tgmessage;
            _database = database;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Task.Run(() =>
            {
                _rabbitMQService.StartListening<Payment_Success_Queue>("payment_to_client", async dtoClaimed =>
                {
                    await InsertMoneyForUser(dtoClaimed.user_id, dtoClaimed.money_value);
                });

                _rabbitMQService.StartListening<Payment_Success_Queue>("payment_to_client_access_moneyback", async dtoClaimedsec =>
                {
                    await DecreaseMoney(dtoClaimedsec.user_id, dtoClaimedsec.money_value);
                });

                _rabbitMQService.StartListening<Payment_Success_Queue>("payment_to_client_error", async dtoClaimedsec =>
                {
                    await InsertMoneyForUserWithError(dtoClaimedsec.user_id, dtoClaimedsec.money_value);
                });

                _rabbitMQService.StartListening<OrderIdsDto>("order_review_queue", async dtoClaimed =>
                {
                    await CreateReview(dtoClaimed);
                });
            }, stoppingToken);



            return Task.CompletedTask;
        }

        protected async Task InsertMoneyForUser(Guid userGUID, long money_value)
        {
            var chatId = _database.GetTelegramChatId(userGUID);

            await _database.InsertMoney(userGUID, money_value);

            var moneyValue = _database.GetUserBalance(userGUID);

            await _tgmessage.SendHtml(chatId, $"Счет успешно пополнен\nВаш баланс: <tg-spoiler>{moneyValue}</tg-spoiler> руб");
        }

        protected async Task DecreaseMoney(Guid userGUID, long money_value)
        {
            var chatId = _database.GetTelegramChatId(userGUID);

            await _database.DecreaseMoney(userGUID, money_value);

            var moneyValue = _database.GetUserBalance(userGUID);


            await _tgmessage.SendHtml(chatId, $"С вашего баланса списаны <tg-spoiler>{money_value}</tg-spoiler> руб\nОстаток баланса: <tg-spoiler>{moneyValue}</tg-spoiler> руб");

        }

        protected async Task InsertMoneyForUserWithError(Guid userGUID, long money_value)
        {
            var chatId = _database.GetTelegramChatId(userGUID);

            await _database.InsertMoney(userGUID, money_value);

            var moneyValue = _database.GetUserBalance(userGUID);

            await _tgmessage.SendHtml(chatId, $"Неправильно была указана карта");

        }

        private async Task CreateReview(OrderIdsDto orderReviewDto)
        {
            ReviewTable review = new ReviewTable
            {
                order_id = orderReviewDto.Id,
                client_id = orderReviewDto.clientId,
                courier_id = orderReviewDto.courier_id,
                restaurant_id = orderReviewDto.restaurant_id,
                rating = 5,
                comment = "",
                review_date = DateTime.UtcNow,
            };

            await _database.CreateReview(review);
        }
    }
}
