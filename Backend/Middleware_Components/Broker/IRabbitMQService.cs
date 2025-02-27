using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Middleware_Components.Broker
{
    public interface IRabbitMQService
    {
        public void SendMessage<T>(string queueName, T message);

        public void StartListening<T>(string queueName, Action<T> onMessageReceived);
    }
}
