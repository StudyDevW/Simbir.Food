using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Middleware_Components.CustomAttributes;
using Middleware_Components.Services;
using ORM_Components;
using ORM_Components.DTO.ClientAPI;
using ORM_Components.DTO.RestaurantAPI;
using RestaurantAPI.Model.Interface;

namespace RestaurantAPI.Model.Controllers
{
    [Authorize(AuthenticationSchemes = "Asymmetric")]
    [ValidateJwt]
    [Route("api/Restaurant/")]
    [ApiController]
    public class RestaurantController : ControllerBase
    {
        private readonly DataContext _dbcontext;
        private readonly IJwtService _jwtServices;
        private readonly IRestaurantServices _restaurantService;


        public RestaurantController(DataContext dbcontext, IJwtService jwtServices, IRestaurantServices restaurantService)
        {
            _dbcontext = dbcontext;
            _jwtServices = jwtServices;
            _restaurantService = restaurantService;
        }

        /// <summary>
        /// Получает список с данными об id ресторана, названии ресторана и средней оценкой этого ресторана.
        /// </summary>
        /// <returns>Список ресторанов и соответствующих оценок.</returns>
        [HttpGet("Get/AverageMarkRestaurant")]
        public async Task<ActionResult<List<RestaurantMark_DTO>>> GetAverageMarkForEveryRestaurant()
        {
            return await _restaurantService.GetRestaurantMark();
        }

        /// <summary>
        /// Отклоняет заказ, принятый в работу в ресторане.
        /// </summary>
        /// <response code="204">Заказ успешно отменён.</response>
        [HttpPost("OrderRejections")]
        public async Task<IActionResult> OrderReject([FromQuery] Guid orderId)
        {
            try
            {
                await _restaurantService.OrderRejections(orderId);
                return Ok("Заказ отменён");
            }
            catch (Exception ex) {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Получает данные о ресторане по его id.
        /// </summary>
        /// <returns>Информацию о ресторане.</returns>
        [HttpGet("{restaurantId}/GetRestaurant")]
        public async Task<ActionResult<Restaurants_DTO>> GetRestaurant(Guid restaurantId)
        {
            return await _restaurantService.GetRestaurant(restaurantId);
        }

        /// <summary>
        /// Получает данные о всех ресторанах.
        /// </summary>
        /// <returns>Информация о всех ресторанах.</returns>
        [HttpGet("GetAllRestaurant")]
        public async Task<ActionResult<List<Restaurants_DTO>>> GetAllRestaurant()
        {
            return await _restaurantService.GetAllRestaurant(Request.Headers["Authorization"], null);
        }

        /// <summary>
        /// Получает данные о ресторанах по поиску.
        /// </summary>
        /// <returns>Информация о всех ресторанах по поиску.</returns>
        [HttpGet("GetRestaurants/{searchText}")]
        public async Task<ActionResult<List<Restaurants_DTO>>> GetAllRestaurantWithSearch(string searchText)
        {
            return await _restaurantService.GetAllRestaurant(Request.Headers["Authorization"], searchText);
        }

        /// <summary>
        /// Изменяет данные ресторана.
        /// </summary>
        /// <response code="204">Данные заказа успешно изменены.</response>
        [HttpPut("{restaurantId}/UpdateRestaurant")]
        public async Task<ActionResult> UpdateRestaurant(Guid restaurantId, [FromBody] RestaurantUpdate_DTO restaurantUpdate_DTO)
        {
            await _restaurantService.UpdateRestaurant(restaurantId, restaurantUpdate_DTO);
            return NoContent();
        }

        /// <summary>
        /// Удаляет ресторан по id.
        /// </summary>
        /// <response code="204">Ресторан успешно удалён.</response>
        [HttpDelete("{restaurantId}/DeleteRestaurant")]
        public async Task<ActionResult> DeleteRestaurant(Guid restaurantId)
        {
            await _restaurantService.DeleteRestaurant(restaurantId);
            return NoContent();
        }

        /// <summary>
        /// Удаляет все рестораны.
        /// </summary>
        /// <response code="204">Все рестораны успешно удалены.</response>
        [HttpDelete("DeleteAllRestaurant")]
        public async Task<ActionResult> DeleteAllRestaurant()
        {
            await _restaurantService.DeleteAllRestaurant();
            return NoContent();
        }

        /// <summary>
        /// Изменяет статус у заказа.
        /// </summary>
        /// <response code="204">Статус заказа изменён.</response>
        [HttpPost("SetReadyStatusForOrder")]
        public async Task<IActionResult> SetReadyStatusForOrder([FromQuery] Guid orderId)
        {
            Console.WriteLine($"Логируем принятие заказа {orderId}.");
            await _restaurantService.SetReadyStatusForOrder(orderId);
            return Ok();

        }
    }
}
