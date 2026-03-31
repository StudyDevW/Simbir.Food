namespace VK_Components.Interfaces
{
    public interface IMessageSender
    {
        public Task Send(string userId, string message);

        public Task SendConfirmationRequest(string userId);

        //public Task SendHtml(string chatId, string message);

        //public Task SendWithMarkup(string chatId, string message, string markupMessage, string markupFlag);
    }
}
