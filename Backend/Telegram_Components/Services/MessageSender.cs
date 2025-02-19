using Telegram.Bot;
using Telegram_Components.Interfaces;

namespace Telegram_Components.Services
{
    public class MessageSender : IMessageSender
    {
        private readonly TelegramBotClient _botClient;
        public MessageSender(string token)
        {
            _botClient = new TelegramBotClient(token);
        }

        public async Task Send(string chatId, string message)
        {
            await _botClient.SendMessage(chatId, message);
        }
    }
}
