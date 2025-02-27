using ClientAPI.Interfaces;
using Middleware_Components.Broker;
using ORM_Components.DTO.CourierAPI;
using ORM_Components.DTO.RestaurantAPI;

namespace ClientAPI.Services
{
    public class RabbitMQListenerService : BackgroundService
    {
        private readonly IRabbitMQService _rabbitMQService;

        public RabbitMQListenerService(IRabbitMQService rabbitMQService)
        {
            _rabbitMQService = rabbitMQService;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Task.Run(() =>
            {
                _rabbitMQService.StartListening<OrderIdsDto>("order_review_queue", orderIdsDto =>
                {
                    //TODO Антон твоя часть
                });
            }, stoppingToken);

            return Task.CompletedTask;
        }
    }
}
