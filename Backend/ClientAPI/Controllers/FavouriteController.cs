using ClientAPI.Interfaces;
using ClientAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Middleware_Components.Services;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;

namespace ClientAPI.Controllers
{
    [Route("api/Favourite/")]
    [ApiController]
    public class FavouriteController : ControllerBase
    {
        private readonly IClientService _clientService;
        private readonly IJwtService _jwt;
        private readonly ILogger _logger;

        public FavouriteController(IClientService clientService, IJwtService jwt, ICacheService cache, IConfiguration configuration)
        {
            _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger("clients-controller-logger");
            _clientService = clientService;
            _jwt = jwt;
        }

        /// <summary>
        /// Добавить ресторан в избранное
        /// </summary>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = "Asymmetric")]
        [HttpPost("{restaurantId}")]
        public async Task<IActionResult> AddRestaurantToFavourite([FromRoute] Guid restaurantId)
        {
            try
            {
                await _clientService.AddRestaurantToFavourite(Request.Headers["Authorization"], restaurantId);
                return Ok("Restaurant added to Favourite.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Удалить ресторан из избранного
        /// </summary>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = "Asymmetric")]
        [HttpDelete("{restaurantId}")]
        public async Task<IActionResult> RemoveRestaurantToFavourite([FromRoute] Guid restaurantId)
        {
            try
            {
                await _clientService.RemoveRestaurantFromFavourite(Request.Headers["Authorization"], restaurantId);
                return Ok("Ресторан добавлен в избранное.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }       
    }
}
