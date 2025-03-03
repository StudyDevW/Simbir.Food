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
                return Ok("Ресторан загружен");
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
        [Route("DeleteAllRestaurantFoodItems")]
        public async Task<IActionResult> DeleteAllRestaurantFoodItems()
        {
            await _restaurantFoodItemsServices.DeleteAllRestaurantFoodItems();
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
        public async Task<List<RestaurantFoodItemsTable>> GetRestaurantFoodItems(Guid restaurantId)
        {
            return await _restaurantFoodItemsServices.GetRestaurantFoodItems(restaurantId);
        }
        [HttpPut]
        [Route("PutRestaurantFoodItems")]
        public async Task<IActionResult> PutRestaurantFoodItems([FromBody] RestaurantFoodItems_DTO restaurantFoodItems_DTO, Guid food_Id)
        {
            if (restaurantFoodItems_DTO == null)
            {
                return BadRequest("Данные блюда не могут быть пустыми.");
            }
            var validation = await _jwtServices.AccessTokenValidation(Request.Headers["Authorization"]);

            if (validation.TokenHasError())
            {
                return Unauthorized();
            }
            else if (validation.TokenHasSuccess())
            {
                var errors = new List<string>();

                if (string.IsNullOrWhiteSpace(restaurantFoodItems_DTO.name))
                {
                    errors.Add("Название блюда не может быть пустым.");
                }
                if (restaurantFoodItems_DTO.price <= 0)
                {
                    errors.Add("Цена блюда не может быть 0.");
                }
                if (restaurantFoodItems_DTO.weight <= 0)
                {
                    errors.Add("Вес блюда не может быть 0.");
                }
                if (restaurantFoodItems_DTO.calories <= 0)
                {
                    errors.Add("Калории блюда не могут быть 0.");
                }

                if (errors.Any())
                {
                    return BadRequest(errors);
                }

                var restaurantFoodItem = await _dbcontext.restaurantFoodItemsTable.FindAsync(restaurantFoodItems_DTO.restaurant_id);
                if (restaurantFoodItem == null)
                {
                    return NotFound("Блюдо с указанным ID не найдено.");
                }

                restaurantFoodItem.name = restaurantFoodItems_DTO.name;
                restaurantFoodItem.price = restaurantFoodItems_DTO.price;
                restaurantFoodItem.image = restaurantFoodItems_DTO.image;
                restaurantFoodItem.weight = restaurantFoodItems_DTO.weight;
                restaurantFoodItem.calories = restaurantFoodItems_DTO.calories;

                _dbcontext.restaurantFoodItemsTable.Update(restaurantFoodItem);
                await _dbcontext.SaveChangesAsync();

                return Ok("Блюдо успешно обновлено.");
            }
            return BadRequest();
            
        }
    }
}
