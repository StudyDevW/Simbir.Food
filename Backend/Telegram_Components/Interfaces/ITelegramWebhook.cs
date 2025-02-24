using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegram_Components.Interfaces
{
    public interface ITelegramWebhook
    {
        public Task WebhookSet(string urlDomain);
    }
}
