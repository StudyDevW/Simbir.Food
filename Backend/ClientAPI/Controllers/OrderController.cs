using ClientAPI.Interfaces;
using MailKit.Search;
using Microsoft.AspNetCore.Mvc;
using Middleware_Components.Services;
using ORM_Components.DTO.ClientAPI.OrderSelecting;
using StackExchange.Redis;

namespace ClientAPI.Controllers
{
    [Route("api/Order/")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IClientService _clientService;
        private readonly IJwtService _jwt;
        private readonly ILogger _logger;

        public OrderController(IClientService clientService, IJwtService jwt, ICacheService cache, IConfiguration configuration)
        {
            _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger("order-controller-logger");
            _clientService = clientService;
            _jwt = jwt;
        }

        /// <summary>
        /// Создать заказ из блюд в корзине
        /// </summary>
        /// <returns></returns>
        [HttpPost("Create")]
        public async Task<IActionResult> CreateOrder()
        {
            try
            {
                await _clientService.CreateOrder(Request.Headers["Authorization"]);
                return Ok("order_created");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //[HttpDelete("Cancel")]
        //public async Task<IActionResult> DeleteOrder()
        //{
        //    return Ok();
        //}

        /// <summary>
        /// Получить свои заказы
        /// </summary>
        /// <returns></returns>
        [HttpGet("Info")]
        public async Task<IActionResult> GetInfoOrders()
        {
            try
            {
                var info = await _clientService.GetAllOrders(Request.Headers["Authorization"]);
                return Ok(info);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Получить свой определенный заказ
        /// </summary>
        /// <returns></returns>
        [HttpGet("Info/{orderId}")]
        public async Task<IActionResult> GetInfoOrderFromId(Guid orderId)
        {
            try
            {
                var info = await _clientService.GetOrderFromId(Request.Headers["Authorization"], orderId);
                return Ok(info);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Получить историю статусов определенного заказа
        /// </summary>
        /// <returns></returns>
        [HttpGet("History/{orderId}")]
        public async Task<IActionResult> HistoryOrder(Guid orderId)
        {
            try
            {
                var info = await _clientService.GetAllHistoryOrder(Request.Headers["Authorization"], orderId);
                return Ok(info);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("InfoForRestaurant")]
        public async Task<ActionResult<List<OrderInfo>>> GetInfosOrdersFromRestaurant([FromQuery] Guid restaurantId)
        {
            try
            {
                if (restaurantId == Guid.Empty)
                    return BadRequest("restaurantId обязателен!");

                if (!Request.Headers.ContainsKey("Authorization"))
                    return Unauthorized("Не передан токен авторизации!");

                var authToken = Request.Headers["Authorization"].ToString();
                var info = await _clientService.GetAllOrdersForRestaurant(authToken, restaurantId, false);
                return Ok(info);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error message: " + ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("InfoForRestaurantOfAllTime")]
        public async Task<ActionResult<List<OrderInfo>>> GetInfosOrdersFromRestaurantOfAllTime([FromQuery] Guid restaurantId)
        {
            try
            {
                if (restaurantId == Guid.Empty)
                    return BadRequest("restaurantId обязателен!");

                if (!Request.Headers.ContainsKey("Authorization"))
                    return Unauthorized("Не передан токен авторизации!");

                var authToken = Request.Headers["Authorization"].ToString();
                var info = await _clientService.GetAllOrdersForRestaurant(authToken, restaurantId, true);
                return Ok(info);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error message: " + ex.Message);
                return BadRequest(ex.Message);
            }
        }
    }
}
