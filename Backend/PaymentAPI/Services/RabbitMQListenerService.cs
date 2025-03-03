using Middleware_Components.Broker;
using ORM_Components.DTO.PaymentAPI;
using PaymentAPI.Interfaces;

namespace PaymentAPI.Services
{
    public class RabbitMQListenerService : BackgroundService
    {
        private readonly IRabbitMQService _rabbitMQService;
        private readonly IPaymentService _paymentService;

        public RabbitMQListenerService(IPaymentService paymentService, IRabbitMQService rabbitMQService)
        {
            _rabbitMQService = rabbitMQService;
            _paymentService = paymentService;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Task.Run(() =>
            {
                _rabbitMQService.StartListening<Payment_Out_Queue>("client_to_payment", async dtoClaimed =>
                {
                    await _paymentService.MoneyBack(new PaymentOut() {
                        money_value = dtoClaimed.money_value,
                        card_number = dtoClaimed.card_number,
                        user_id = dtoClaimed.user_id
                    });
                });

            }, stoppingToken);

            return Task.CompletedTask;
        }
    }
}
