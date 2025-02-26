using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
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
            await _botClient.SendMessage(chatId, message, ParseMode.Markdown);
        }

        public async Task SendWithMarkup(string chatId, string message, string markupMessage, string markupFlag)
        {

            var replyMarkup = new InlineKeyboardMarkup(new[]
            {
                new[] { InlineKeyboardButton.WithCallbackData(markupMessage, markupFlag) }
            });

            await _botClient.SendMessage(chatId, message,
              replyMarkup: replyMarkup);

        }
    }
}
