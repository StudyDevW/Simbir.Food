namespace Telegram_Components.Interfaces
{
    public interface IMessageSender
    {
        public Task Send(string chatId, string message);
    }
}
