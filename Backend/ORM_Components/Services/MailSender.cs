using MailKit.Net.Smtp;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using ORM_Components.DTO.MailDtos;
using ORM_Components.Interfaces;
using System.Text;

namespace ORM_Components.Services
{
    public class MailSender : IMailSender
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly DataContext _dataContext;

        public MailSender(IConfiguration configuration) 
        {
            _configuration = configuration;
            _dataContext = new DataContext(configuration["DATABASE_CONNECT"]);
            _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger("MailSender | database-sdk-logger");
        }

        public async Task SendEmailAsync(EmailDto emailDto)
        {
            _logger.LogInformation($"Отправляю сообщение на почту {emailDto.email}...");

            if (emailDto == null) { throw new Exception("В MailSender пришла пустая ДТОшка."); }

            var order = await _dataContext.orderTable
                .FirstOrDefaultAsync(x => x.Id == emailDto.orderId)
                ?? throw new Exception("Заказ не найден.");

            string restaurantName = await _dataContext.restaurantTable
                .Where(x => x.Id == order.restaurant_id)
                .Select(x => x.restaurantName)
                .FirstOrDefaultAsync()
                ?? throw new Exception("Неизвестный ресторан.");

            var foodItems = await _dataContext.orderItemsTable
            .Where(oi => oi.order_id == emailDto.orderId)
            .Join(
                _dataContext.restaurantFoodItemsTable,
                orderItem => orderItem.restaraunt_food_item,
                foodItem => foodItem.Id,                     
                (orderItem, foodItem) => foodItem            
            )
            .ToListAsync();

            var emailMessage = new MimeMessage();

            var subject = "Чек заказа";

            var senderName = _configuration["SENDERNAME"] ?? throw new Exception("SENDERNAME не задан");
            var senderEmail = _configuration["SENDEREMAIL"] ?? throw new Exception("SENDEREMAIL не задан");

            emailMessage.From.Add(new MailboxAddress(senderName, senderEmail));
            emailMessage.To.Add(MailboxAddress.Parse(emailDto.email));
            emailMessage.Subject = subject;

            StringBuilder message = new StringBuilder();
            message.AppendLine($"***************** Чек для заказа: {emailDto.orderId} *****************");
            message.AppendLine($"Заказ готовил ресторан: {restaurantName}");
            message.AppendLine("Состав заказа: ");
            foreach (var item in foodItems)
            {
                message.AppendLine($"- {item.name}: {item.price}₽");
            }
            message.AppendLine("----------------------------");
            message.AppendLine($"Итоговая сумма заказа: {order.total_price}");
            message.AppendLine($"Дата создания заказ: {order.order_date}");

            var htmlMessage = message.ToString()
            .Replace(Environment.NewLine, "<br>")
            .Replace("\n", "<br>")
            .Replace("\r", "");

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $"<html><head><meta charset='UTF-8'></head><body>{htmlMessage}</body></html>",
                TextBody = message.ToString()
            };

            emailMessage.Body = bodyBuilder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                try
                {
                    if (!int.TryParse(_configuration["MAILPORT"], out int mailPort))
                        throw new Exception("MAILPORT указан некорректно");

                    if (!bool.TryParse(_configuration["USESSL"], out bool useSsl))
                        useSsl = true;

                    await client.ConnectAsync(_configuration["MAILSERVER"], int.Parse(_configuration["MAILPORT"]), bool.Parse(_configuration["USESSL"]));
                    await client.AuthenticateAsync(senderEmail, _configuration["EMAILPASSWORD"]);
                    await client.SendAsync(emailMessage);
                    await client.DisconnectAsync(true);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при отправке email");
                    throw;
                }
            }
        }
    }
}
