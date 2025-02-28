using ClientAPI.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Middleware_Components.Services;
using ORM_Components.DTO.ClientAPI;

namespace ClientAPI.Controllers
{
    [Route("api/Basket/")]
    
    [ApiController]
    public class BasketController : ControllerBase
    {
        private readonly IClientService _clientService;
        private readonly IJwtService _jwt;
        private readonly ILogger _logger;

        public BasketController(IClientService clientService, IJwtService jwt, ICacheService cache, IConfiguration configuration)
        {
            _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger("clients-controller-logger");
            _clientService = clientService;
            _jwt = jwt;
        }

        [Authorize(AuthenticationSchemes = "Asymmetric")]
        [HttpPost]
        public async Task<IActionResult> AddItem([FromBody] Basket_Add dtoObj)
        {
            try
            {
                await _clientService.AddBasketItem(Request.Headers["Authorization"], dtoObj);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);  
            }
        }

        //[HttpPost("CreateOrder")]
        //public async Task<IActionResult> AddOrder()
        //{
        //    return Ok();
        //}

        [HttpGet("All")]
        public async Task<IActionResult> GetAllItemsRestaurant()
        {
            try
            {
                var itemsBasket = await _clientService.GetItemsBasket(Request.Headers["Authorization"]);
                
                if (itemsBasket != null)
                {
                    return Ok(itemsBasket);
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

            return BadRequest();
        }

        [HttpDelete("{backetId}")] //id записи корзины
        public async Task<IActionResult> DeleteItem(Guid backetId)
        {

            return Ok();
        }

        [HttpDelete] //Вся корзина по пользователю
        public async Task<IActionResult> DeleteItems()
        {

            return Ok();
        }
    }
}
