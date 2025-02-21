using Microsoft.AspNetCore.Mvc;
using Middleware_Components.Services;
using ORM_Components.DTO.RestaurantAPI;
using Telegram_Components.Interfaces;

namespace RestaurantAPI.Model.Controllers
{
    [Route("api/OrdersDeviation")]
    [ApiController]
    public class OrdersDeviationController : ControllerBase
    {
        private readonly IMessageSender _messageSender;
        private readonly IJwtService _jwtServices;

        public OrdersDeviationController (IJwtService jwtServices, IMessageSender messageSender)
        {
            _jwtServices = jwtServices;
            _messageSender = messageSender;

        }

        [HttpPost("SentOrder")]
        public async Task<IActionResult> SentOrder([FromBody] Order_DTO order_DTO)
        {
            if (order_DTO == null)
            {
                return BadRequest("Неверные данные заказа.");
            }

            var validation = await _jwtServices.AccessTokenValidation(Request.Headers["Authorization"]);

            if (validation.TokenHasError())
            {
                return Unauthorized();
            }
            else if (validation.TokenHasSuccess())
            {
               

                string message = $"Ваш заказ в ресторане {order_DTO.restaurant_id} был отклонен.\n" +
                                 $"Сумма заказа: {order_DTO.total_price}.\n" +
                                 $"Дата заказа: {order_DTO.order_date}.";

                await _messageSender.Send(validation.token_success.telegramChatId, message);

                return Ok("Заказ отклонен и отправлено уведомление.");
            }
            return BadRequest();
            
        }
    }
}