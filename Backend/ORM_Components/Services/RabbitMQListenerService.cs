using Middleware_Components.Broker;
using ORM_Components.DTO.ClientAPI;
using ORM_Components.DTO.MailDtos;
using ORM_Components.DTO.PaymentAPI;
using ORM_Components.Interfaces;
using System.Text.Json;

namespace ORM_Components.Services
{
    public class RabbitMQListenerService : BackgroundService
    {
        private readonly IMailSender _mailSender;
        private readonly IRabbitMQService _rabbitMQService;

        public RabbitMQListenerService(IMailSender mailSender, IRabbitMQService rabbitMQService) 
        {
            _mailSender = mailSender;
            _rabbitMQService = rabbitMQService;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Task.Run(() =>
            {
                _rabbitMQService.StartListening<EmailDto>("courier_to_orm", async mailDto =>
                {
                    await _mailSender.SendEmailAsync(mailDto);
                });


            }, stoppingToken);



            return Task.CompletedTask;
        }
    }
}
