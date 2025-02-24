using ClientAPI.Interfaces;
using Middleware_Components.Broker;
using ORM_Components.DTO.CourierAPI;
using ORM_Components.DTO.RestaurantAPI;

namespace ClientAPI.Services
{
    public class RabbitMQListenerService : BackgroundService
    {
        private readonly RabbitMQService _rabbitMQService;
        private readonly IDatabaseService _dataService;

        public RabbitMQListenerService(RabbitMQService rabbitMQService, IDatabaseService dataService)
        {
            _rabbitMQService = rabbitMQService;
            _dataService = dataService;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Task.Run(() =>
            {
                _rabbitMQService.StartListening<OrderIdsDto>("order_review_queue", orderIdsDto =>
                {
                    //TODO Антон твоя часть
                });
                _rabbitMQService.StartListening<CourierDto>("test_courier_client", courierDto =>
                {
                    //Пример 
                    Console.WriteLine($"-----Получено сообщение из CourierAPI-----\n" +
                        $"Id: {courierDto.Id};\n" +
                        $"UserId: {courierDto.userId};\n" +
                        $"Car_number: {courierDto.car_number};\n" +
                        $"Status: {courierDto.status}");
                });
            }, stoppingToken);

            return Task.CompletedTask;
        }
    }
}
