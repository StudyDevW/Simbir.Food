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
    [Route("api/Restaurant/")]
    [ApiController]
    public class RestaurantController : ControllerBase
    {
        private readonly DataContext _dbcontext;
        private readonly IJwtService _jwtServices;

        public RestaurantController(DataContext dbcontext, IJwtService jwtServices)
        {
            _dbcontext = dbcontext;
            _jwtServices = jwtServices;
        }

        [HttpPost]
        [Route("SentRestaurant")]
        public async Task<IActionResult> AddRestaurant([FromBody] Restaurants_DTO restaurant_DTO)
        {
            var validation = await _jwtServices.AccessTokenValidation(Request.Headers["Authorization"]);

            if (validation.TokenHasError())
            {
                return Unauthorized();
            }
            else if (validation.TokenHasSuccess())
            {
                if (validation.token_success.userRoles.Contains("Admin"))
                {
                    return Forbid("У вас нет прав для создания ресторана.");
                }
                if (validation.token_success.userRoles.Contains("Courier"))
                {
                    return Forbid("У вас нет прав для создания ресторана.");
                }
                if (restaurant_DTO == null)
                {
                    return BadRequest("Данные ресторана не могут быть пустыми.");
                }

                if (string.IsNullOrWhiteSpace(restaurant_DTO.restaurantName))
                {
                    return BadRequest("Название ресторана не может быть пустым.");
                }

                if (string.IsNullOrWhiteSpace(restaurant_DTO.address))
                {
                    return BadRequest("Адрес ресторана не может быть пустым.");
                }

                if (string.IsNullOrWhiteSpace(restaurant_DTO.phone_number))
                {
                    return BadRequest("Номер телефона ресторана не может быть пустым.");
                }

                RestaurantTable restaurantTable = new RestaurantTable()
                {
                    user_id = restaurant_DTO.user_id,
                    restaurantName = restaurant_DTO.restaurantName,
                    address = restaurant_DTO.address,
                    phone_number = restaurant_DTO.phone_number,
                    status = restaurant_DTO.status,
                    description = restaurant_DTO.description,
                    imagePath = restaurant_DTO.imagePath,
                    open_time = restaurant_DTO.open_time,
                    close_time = restaurant_DTO.close_time
                };

                _dbcontext.restaurantTable.Add(restaurantTable);
                await _dbcontext.SaveChangesAsync();
                return Ok("Успех");
            }
            return BadRequest();
        }


        [HttpDelete]
        [Route("DeleteRestaurant/{id}")]
        public async Task<IActionResult> DeleteRestaurant(Guid id)
        {
            var validation = await _jwtServices.AccessTokenValidation(Request.Headers["Authorization"]);

            if (validation.TokenHasError())
            {
                return Unauthorized();
            }
            else if (validation.TokenHasSuccess())
            {
                var restaurant = await _dbcontext.restaurantTable.FindAsync(id);
                if (restaurant == null)
                {
                    return NotFound("Ресторан не найден.");
                }

                _dbcontext.restaurantTable.Remove(restaurant);
                await _dbcontext.SaveChangesAsync();
                return Ok("Ресторан успешно удалён");
            }
            return BadRequest();

            
        }

        [HttpGet]
        [Route("GetRestaurant")]
        public async Task<IActionResult> GetRestaurant()
        {
            var validation = await _jwtServices.AccessTokenValidation(Request.Headers["Authorization"]);

            if (validation.TokenHasError())
            {
                return Unauthorized();
            }
            else if (validation.TokenHasSuccess())
            {
                var restaurants = await _dbcontext.restaurantTable.ToListAsync();
                return Ok(restaurants);
            }
            return BadRequest();
            
        }

        [HttpGet]
        [Route("GetRestaurant/{id}")]
        public async Task<IActionResult> GetRestaurantById(Guid id)
        {
            var validation = await _jwtServices.AccessTokenValidation(Request.Headers["Authorization"]);

            if (validation.TokenHasError())
            {
                return Unauthorized();
            }
            else if (validation.TokenHasSuccess())
            {
                var restaurant = await _dbcontext.restaurantTable.FindAsync(id);
                if (restaurant == null)
                {
                    return NotFound("Ресторан не найден.");
                }

                return Ok(restaurant);
            }
            return BadRequest();
            
        }

        [HttpPut]
        [Route("PutRestaurant/{id}")]
        public async Task<IActionResult> PutRestaurant(Guid id, [FromBody] Restaurants_DTO restaurants_DTO)
        {
            var validation = await _jwtServices.AccessTokenValidation(Request.Headers["Authorization"]);

            if (validation.TokenHasError())
            {
                return Unauthorized();
            }
            else if (validation.TokenHasSuccess())
            {
                if (restaurants_DTO == null)
                {
                    return BadRequest("Данные ресторана не могут быть пустыми.");
                }

                if (string.IsNullOrWhiteSpace(restaurants_DTO.restaurantName))
                {
                    return BadRequest("Название ресторана не может быть пустым.");
                }

                if (string.IsNullOrWhiteSpace(restaurants_DTO.address))
                {
                    return BadRequest("Адрес ресторана не может быть пустым.");
                }

                var restaurant = await _dbcontext.restaurantTable.FindAsync(id);
                if (restaurant == null)
                {
                    return NotFound("Ресторан не найден.");
                }

                restaurant.restaurantName = restaurants_DTO.restaurantName;
                restaurant.address = restaurants_DTO.address;
                restaurant.phone_number = restaurants_DTO.phone_number;
                restaurant.status = restaurants_DTO.status;
                restaurant.description = restaurants_DTO.description;
                restaurant.imagePath = restaurants_DTO.imagePath;
                restaurant.open_time = restaurants_DTO.open_time;
                restaurant.close_time = restaurants_DTO.close_time;

                _dbcontext.restaurantTable.Update(restaurant);
                await _dbcontext.SaveChangesAsync();
                return Ok("Данные ресторана успешно обновлены.");
            }
            return BadRequest();    
        }
    }
}
