using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Middleware_Components.Services;
using ORM_Components;
using ORM_Components.DTO.ClientAPI;
using ORM_Components.DTO.RestaurantAPI;
using ORM_Components.Tables;
using System.Net;


namespace RestaurantAPI.Model.Controllers
{
    [Authorize(AuthenticationSchemes = "Asymmetric")]
    [Route("api/RestaurantFoodItems/")]
    [ApiController]
    public class RestaurantFoodItemsController : ControllerBase
    {
        private readonly DataContext _dbcontext;
        private readonly IJwtService _jwtServices;

        public RestaurantFoodItemsController(DataContext dbcontext, IJwtService jwtServices)
        {
            _dbcontext = dbcontext;
            _jwtServices = jwtServices;
        }

        [HttpPost]
        [Route("AddRestaurantFoodItems")]
        public async Task<IActionResult> AddRestaurantFoodItems([FromBody] RestaurantFoodItems_DTO restaurantFoodItems_DTO)
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

                var restaurantExists = await _dbcontext.restaurantTable.FindAsync(restaurantFoodItems_DTO.restaurant_id);
                if (restaurantExists == null)
                {
                    return BadRequest("Ресторан с указанным ID не найден.");
                }

                RestaurantFoodItemsTable restaurantFoodItemsTable = new RestaurantFoodItemsTable()
                {
                    restaurant_id = restaurantFoodItems_DTO.restaurant_id,
                    name = restaurantFoodItems_DTO.name,
                    price = restaurantFoodItems_DTO.price,
                    image = restaurantFoodItems_DTO.image,
                    weight = restaurantFoodItems_DTO.weight,
                    calories = restaurantFoodItems_DTO.calories
                };

                _dbcontext.restaurantFoodItemsTable.Add(restaurantFoodItemsTable);
                await _dbcontext.SaveChangesAsync();
                return Ok("Успех");
            }
            return BadRequest();
            
        }

        [HttpDelete]
        [Route("DeleteRestaurantFoodItems/{id}")]
        public async Task<IActionResult> DeleteRestaurantFoodItems(Guid id)
        {
            var validation = await _jwtServices.AccessTokenValidation(Request.Headers["Authorization"]);

            if (validation.TokenHasError())
            {
                return Unauthorized();
            }
            else if (validation.TokenHasSuccess())
            {
                var restaurantFoodItems = await _dbcontext.restaurantFoodItemsTable.FindAsync(id);
                if (restaurantFoodItems == null)
                {
                    return NotFound("Ресторан не найден.");
                }

                _dbcontext.restaurantFoodItemsTable.Remove(restaurantFoodItems);
                await _dbcontext.SaveChangesAsync();
                return Ok("Ресторан успешно удалён");
            }
            return BadRequest();
            
        }
        [HttpGet]
        [Route("GetRestaurantFoodItems")]
        public async Task<IActionResult> GetRestaurantFoodItems()
        {
            var validation = await _jwtServices.AccessTokenValidation(Request.Headers["Authorization"]);

            if (validation.TokenHasError())
            {
                return Unauthorized();
            }
            else if (validation.TokenHasSuccess())
            {
                var restaurantFoodItems = await _dbcontext.restaurantFoodItemsTable.ToListAsync();
                return Ok(restaurantFoodItems);
            }
            return BadRequest();
           
        }

        [HttpPut]
        [Route("PutRestaurantFoodItems")]
        public async Task<IActionResult> PutRestaurantFoodItems(Guid food_id, [FromBody] RestaurantFoodItems_DTO restaurantFoodItems_DTO)
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

                var restaurantFoodItem = await _dbcontext.restaurantFoodItemsTable.FindAsync(food_id);
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
