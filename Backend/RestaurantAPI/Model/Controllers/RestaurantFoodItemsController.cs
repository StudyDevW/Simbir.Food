using CourierAPI.Controllers.CustomAttributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Middleware_Components.Services;
using ORM_Components;
using ORM_Components.DTO.ClientAPI;
using ORM_Components.DTO.RestaurantAPI;
using ORM_Components.Tables;
using RestaurantAPI.Model.GetRastaurant;
using RestaurantAPI.Model.Interface;
using RestaurantAPI.Model.Services;
using System.Net;


namespace RestaurantAPI.Model.Controllers
{
    [Authorize(AuthenticationSchemes = "Asymmetric")]
    [ValidateJwt]
    [Route("api/RestaurantFoodItems/")]
    [ApiController]
    public class RestaurantFoodItemsController : ControllerBase
    {
        private readonly IRestaurantFoodItemsServices _restaurantFoodItemsServices;
        private readonly DataContext _dbcontext;
        private readonly IJwtService _jwtServices;

        public RestaurantFoodItemsController(DataContext dbcontext, IJwtService jwtServices, IRestaurantFoodItemsServices restaurantFoodItemsServices)
        {
            _dbcontext = dbcontext;
            _jwtServices = jwtServices;
            _restaurantFoodItemsServices = restaurantFoodItemsServices;
        }

        [HttpPost]
        [Route("AddRestaurantFoodItems")]
        public async Task<IActionResult> AddRestaurantFoodItems([FromBody] RestaurantFoodItems_DTO restaurantFoodItems_DTO)
        {
            try
            {
                await _restaurantFoodItemsServices.AddRestaurantFoodItems(restaurantFoodItems_DTO);
                return Ok("Блюдо успешно добавлено.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpDelete]
        [Route("DeleteRestaurantFoodItems/{id}")]
        public async Task<IActionResult> DeleteRestaurantFoodItems(Guid id)
        {
            await _restaurantFoodItemsServices.DeleteRestaurantFoodItems(id);
            return NoContent();
        }
        [HttpDelete]
        [Route("DeleteAllRestaurantFoodItems/{id}")]
        public async Task<IActionResult> DeleteAllRestaurantFoodItems(Guid id)
        {
            await _restaurantFoodItemsServices.DeleteAllRestaurantFoodItems(id);
            return NoContent();
        }
        [HttpGet]
        [Route("GetRestaurantFoodItems")]
        public async Task<List<RestaurantFoodItemsTable>> GetAllRestaurantFoodItems()
        {
            return await _restaurantFoodItemsServices.GetAllRestaurantFoodItems();
        }

        [HttpGet]
        [Route("GetRestaurantFoodItems/{id}")]
        public async Task<IActionResult> GetRestaurantFoodItems(Guid id)
        {
            return Ok(await _restaurantFoodItemsServices.GetRestaurantFoodItems(id));
        }

        [HttpPut]
        [Route("PutRestaurantFoodItems")]
        public async Task<IActionResult> PutRestaurantFoodItems([FromBody] RestaurantFoodItems_DTO restaurantFoodItems_DTO, Guid food_Id)
        {
            await _restaurantFoodItemsServices.PutRestaurantFoodItems( restaurantFoodItems_DTO, food_Id);
            return NoContent();
        }
    }
}
