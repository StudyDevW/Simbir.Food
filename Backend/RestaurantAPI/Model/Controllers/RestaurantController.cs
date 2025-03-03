
using Microsoft.AspNetCore.Mvc;
using Middleware_Components.Services;
using ORM_Components;
using ORM_Components.DTO.ClientAPI;
using ORM_Components.DTO.RestaurantAPI;
using RestaurantAPI.Model.Interface;

namespace RestaurantAPI.Model.Controllers
{
    [ApiController]
    //[ValidateJwt]
    [Route("api/Restaurant/")]
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
        [HttpGet("Get/AverageMarkRestaurant")]
        public async Task<ActionResult<List<RestaurantMark_DTO>>> GetAverageMarkForEveryRestaurant(Order_DTO order)
        {
            await _restaurantService.OrderRejections(order);
            return await _restaurantService.GetRestaurantMark();
        }
        [HttpPost("OrderRejections")]
        public async Task<IActionResult> OrderReject(Order_DTO order_DTO)
        {
            try
            {
                await _restaurantService.OrderRejections(order_DTO);
                return Ok("Заказ отменён");
            }
            catch (Exception ex) {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("{restaurantId}/GetRestaurant")]
        public async Task<ActionResult<Restaurants_DTO>> GetRestaurant(Guid restaurantId)
        {
            return await _restaurantService.GetRestaurant(restaurantId);
        }

        [HttpGet("GetAllRestaurant")]
        public async Task<ActionResult<List<Restaurants_DTO>>> GetAllRestaurant()
        {
            return await _restaurantService.GetAllRestaurant();
        }

        [HttpPost("CreateRestaurant")]
        public async Task<ActionResult> CreateRestaurant(Guid restaurantId)
        {
            await _restaurantService.CreateRestaurant(restaurantId);
            return NoContent();
        }
        [HttpPut("{restaurantId}/UpdateRestaurant")]
        public async Task<ActionResult> UpdateRestaurant(Guid restaurantId, [FromBody] RestaurantUpdate_DTO restaurantUpdate_DTO)
        {
            await _restaurantService.UpdateRestaurant(restaurantId, restaurantUpdate_DTO);
            return NoContent();
        }

        [HttpDelete("{restaurantId}/DeleteRestaurant")]
        public async Task<ActionResult> DeleteRestaurant(Guid restaurantId)
        {
            await _restaurantService.DeleteRestaurant(restaurantId);
            return NoContent();
        }

        [HttpDelete("DeleteAllRestaurant")]
        public async Task<ActionResult> DeleteAllRestaurant()
        {
            await _restaurantService.DeleteAllRestaurant();
            return NoContent();
        }
        [HttpPost("{orderId}/SetReadyStatusForOrder")]
        public async Task<ActionResult> SetReadyStatusForOrder(Guid orderId)
        {
            await _restaurantService.SetReadyStatusForOrder(orderId);
            return NoContent();
        }
    }
}
