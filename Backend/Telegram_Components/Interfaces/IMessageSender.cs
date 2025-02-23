namespace Telegram_Components.Interfaces
{
    public interface IMessageSender
    {
        public Task Send(string chatId, string message);

        public Task SendWithMarkup(string chatId, string message, string markupMessage, string markupFlag);
    }
}
