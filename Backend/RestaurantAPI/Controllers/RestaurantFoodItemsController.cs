using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Middleware_Components.CustomAttributes;
using Middleware_Components.Services;
using ORM_Components;
using ORM_Components.DTO.ClientAPI;
using ORM_Components.DTO.RestaurantAPI;
using ORM_Components.Tables;
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

        /// <summary>
        /// Добавляет блюдо в ресторан.
        /// </summary>
        /// <response code="204">Блюдо добавлено в ресторан.</response>
        [HttpPost]
        [Route("AddRestaurantFoodItems")]
        public async Task<IActionResult> AddRestaurantFoodItems([FromBody] RestaurantFoodItemsDtoForCreate restaurantFoodItemsDtoForCreate)
        {
            await _restaurantFoodItemsServices.AddRestaurantFoodItems(restaurantFoodItemsDtoForCreate);
            return Ok("Блюдо успешно добавлено.");
        }

        /// <summary>
        /// Удаляет блюдо в ресторан.
        /// </summary>
        /// <response code="204">Блюдо из ресторана удалено.</response>
        [HttpDelete]
        [Route("DeleteRestaurantFoodItems/{id}")]
        public async Task<IActionResult> DeleteRestaurantFoodItems(Guid id)
        {
            await _restaurantFoodItemsServices.DeleteRestaurantFoodItems(id);
            return NoContent();
        }

        /// <summary>
        /// Удаляет все блюда из ресторана.
        /// </summary>
        /// <response code="204">Меню ресторана очищено.</response>
        [HttpDelete]
        [Route("DeleteAllRestaurantFoodItems/{id}")]
        public async Task<IActionResult> DeleteAllRestaurantFoodItems(Guid id)
        {
            await _restaurantFoodItemsServices.DeleteAllRestaurantFoodItems(id);
            return NoContent();
        }

        /// <summary>
        /// Получает данные о всех существующих блюдах.
        /// </summary>
        /// <returns>Получает все данные обо всех существующих блюдах.</returns>
        [HttpGet]
        [Route("GetRestaurantFoodItems")]
        public async Task<List<RestaurantFoodItemsDto>> GetAllRestaurantFoodItems()
        {
            return await _restaurantFoodItemsServices.GetAllRestaurantFoodItems();
        }

        /// <summary>
        /// Получает данные о всех блюдах из ресторана.
        /// </summary>
        /// <returns>Получает все данные обо всех блюдах доступных в ресторане.</returns>
        [HttpGet]
        [Route("GetRestaurantFoodItems/{id}")]
        public async Task<IActionResult> GetRestaurantFoodItems(Guid id)
        {
            return Ok(await _restaurantFoodItemsServices.GetRestaurantFoodItems(id));
        }

        /// <summary>
        /// Изменяет данные блюда.
        /// </summary>
        /// <response code="204">Данные блюда изменены.</response>
        [HttpPut]
        [Route("PutRestaurantFoodItems")]
        public async Task<IActionResult> PutRestaurantFoodItems(Guid food_Id, [FromBody] RestaurantFoodItemsDtoForUpdate restaurantFoodItemsDtoForUpdate)
        {
            await _restaurantFoodItemsServices.UpdateRestaurantFoodItems(food_Id, restaurantFoodItemsDtoForUpdate);
            return NoContent();
        }
    }
}
