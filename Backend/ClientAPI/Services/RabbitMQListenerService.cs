using Middleware_Components.Broker;

namespace ClientAPI.Services
{
    public class RabbitMQListenerService : BackgroundService
    {
        private readonly RabbitMQService _rabbitMQService;
        private readonly DatabaseService _dataService;
        private readonly ILogger<RabbitMQListenerService> _logger;

        public RabbitMQListenerService(RabbitMQService rabbitMQService, DatabaseService dataService, ILogger<RabbitMQListenerService> logger)
        {
            _rabbitMQService = rabbitMQService;
            _dataService = dataService;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Task.Run(() =>
            {
                _rabbitMQService.StartListening<Guid>("order_review_queue", orderId =>
                {
                    _logger.LogInformation($"Получено сообщение с OrderId: {orderId}");
                    _dataService.ReviewForOrder(orderId);
                });
            }, stoppingToken);

            return Task.CompletedTask;
        }
    }
}
