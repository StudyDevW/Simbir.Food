using Telegram.Bot.Types;
using Telegram.Bot;

namespace Telegram_Components.Interfaces
{
    public interface IMessageReceiver
    {
        public Task handleCallbackQuery(CallbackQuery query);

        public Task handleMessage(Message message);

    }
}
