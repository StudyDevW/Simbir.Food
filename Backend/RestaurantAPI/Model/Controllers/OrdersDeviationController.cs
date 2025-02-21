using Microsoft.AspNetCore.Mvc;
using Middleware_Components.Services;
using ORM_Components.DTO.RestaurantAPI;

namespace RestaurantAPI.Model.Controllers
{
    [Route("api/OrdersDeviation")]
    [ApiController]
    public class OrdersDeviationController : ControllerBase
    {
        private readonly IJwtService _jwtServices;

        public OrdersDeviationController (IJwtService jwtServices)
        {
            _jwtServices = jwtServices;
        }

        [HttpPost("SentOrder")]
        public async Task<IActionResult> SentOrder([FromBody] Order_DTO order_DTO)
        {
            if (order_DTO == null)
            {
                return BadRequest("Неверные данные заказа.");
            }

            string message = $"Ваш заказ в ресторане {order_DTO.restaurant_id} был отклонен.\n" +
                             $"Сумма заказа: {order_DTO.total_price}.\n" +
                             $"Дата заказа: {order_DTO.order_date}.";

            await _telegramChatId.Send(order_DTO.client_id, message);

            return Ok("Заказ отклонен и отправлено уведомление.");
        }
    }
}