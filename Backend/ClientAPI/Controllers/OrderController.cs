using ClientAPI.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Middleware_Components.Services;

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

        [HttpPost("CreateOrder")]
        public async Task<IActionResult> CreateOrder()
        {
            return Ok();
        }

        [HttpDelete("CancelOrder")]
        public async Task<IActionResult> DeleteOrder()
        {
            return Ok();
        }

        [HttpGet("OrdersInfo")]
        public async Task<IActionResult> GetInfoOrder()
        {
            return Ok();
        }

        [HttpGet("OrderInfo/{id}")]
        public async Task<IActionResult> GetInfoOrderFromId()
        {
            return Ok();
        }


    }
}
