using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram_Components.Interfaces;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.ReplyMarkups;
using Middleware_Components.Services;
using ORM_Components.DTO.ClientAPI;

namespace Telegram_Components.Services
{
    public class MessageReceiver : IMessageReceiver
    {
        private readonly ITelegramBotClient _botClient;
        private readonly IDatabaseOperations _databaseOperations;
        private readonly ICacheService _cache;
        public MessageReceiver(ITelegramBotClient botClient, IDatabaseOperations databaseOperations, ICacheService cache) {

            _botClient = botClient;
            _cache = cache;
            _databaseOperations = databaseOperations;
        }

        public async Task handleCallbackQuery(CallbackQuery query)
        {
            //todo: integration test in clientservice register
            if (query.Data == null || query.Message == null)
                return;

            await _botClient.AnswerCallbackQuery(query.Id); //Убирает сообщение Загрузка...

            if (query.Data.Equals("itsnotmeQuery"))
            {
                await _botClient.EditMessageText(query.From.Id, query.Message.Id, "Меры приняты, завершение сессии");
            }

            if (query.Data.Equals("registerQuery"))
            {
                if (_cache.CheckExistKeysStorage<AuthAddUser>($"register_request_{query.From.Id}"))
                {
                    var dtoCached = _cache.GetKeyFromStorage<AuthAddUser>($"register_request_{query.From.Id}");

                    await _databaseOperations.AddUserFromTelegram(dtoCached);

                    var replyMarkup = new InlineKeyboardMarkup(new[]
                    {
                        new[] { InlineKeyboardButton.WithUrl("Открыть приложение", "https://t.me/SimbirFoodbot/MiniApp") }
                    });

                    await _botClient.EditMessageText(query.From.Id, query.Message.Id, "Регистрация успешна", 
                        replyMarkup: replyMarkup);

                    _cache.DeleteKeyFromStorage($"register_request_{query.From.Id}");

                }
                else
                {
                    await _botClient.EditMessageText(query.From.Id, query.Message.Id, "Ошибка регистрации, возможно истекло время заявки регистрации");
                }
            }

            //if (query.Data.Equals("AcceptButtonQuery"))
            //{
            //    await _botClient.DeleteMessage(query.From.Id, query.Message.Id);
            //}

            if (query.Data.Equals("testQuery"))
            {
                await _botClient.DeleteMessage(query.From.Id, query.Message.Id);
            }
        }

        public async Task handleMessage(Message message)
        {
            //Эхо
            if (message.Type == MessageType.Text)
            {
                var replyMarkup = new InlineKeyboardMarkup(new[]
                {
                    new[] { InlineKeyboardButton.WithCallbackData("Хорошо", "testQuery") }
                });

                await _botClient.SendMessage(message.Chat.Id, $"Команда: \"{message.Text}\" не распознана!",
                    replyMarkup: replyMarkup);
            }
        }
    }
}
