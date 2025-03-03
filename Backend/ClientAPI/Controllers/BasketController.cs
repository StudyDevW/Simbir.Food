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
            _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger("basket-controller-logger");
            _clientService = clientService;
            _jwt = jwt;
        }

        /// <summary>
        /// Добавление товара в корзину
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Получить все товары из корзины
        /// </summary>
        /// <returns></returns>
        [HttpGet]
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

        /// <summary>
        /// Удалить один элемент из корзины
        /// </summary>
        /// <returns></returns>
        [HttpDelete("{id}")] //id записи корзины
        public async Task<IActionResult> DeleteItem(Guid id)
        {
            try
            {
                await _clientService.DeleteOneBasketItem(Request.Headers["Authorization"], id);
                return Ok("item_deleted");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Удалить корзину целиком
        /// </summary>
        /// <returns></returns>
        [HttpDelete] //Вся корзина по пользователю
        public async Task<IActionResult> DeleteItems()
        {
            try
            {
                await _clientService.DeleteAllBasket(Request.Headers["Authorization"]);
                return Ok("items_deleted");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
