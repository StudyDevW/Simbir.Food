using ClientAPI.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Middleware_Components.Services;
using ORM_Components.DTO.ClientAPI;

namespace ClientAPI.Controllers
{
    [Route("api/Freeze/")]
    [ApiController]
    public class FreezeController : ControllerBase
    {

        private readonly IClientService _clientService;
        private readonly IJwtService _jwt;
        private readonly ILogger _logger;

        public FreezeController(IClientService clientService, IJwtService jwt, ICacheService cache, IConfiguration configuration)
        {
            _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger("frozen-controller-logger");
            _clientService = clientService;
            _jwt = jwt;
        }

        [HttpPost("Restaurant/{id}")]
        public async Task<IActionResult> DowngradeRestaurant(Guid id, Downgrade dtoObj)
        {

            try
            {
                await _clientService.FreezeWorkRestaurantWithAdmin(Request.Headers["Authorization"], id, dtoObj);
                return Ok("restaurant_was_frozen");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("Courier/{id}")]
        public async Task<IActionResult> DowngradeCourier(Guid id, Downgrade dtoObj)
        {
            try
            {
                await _clientService.FreezeWorkCourierWithAdmin(Request.Headers["Authorization"], id, dtoObj);
                return Ok("courier_was_frozen");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("Restaurant/{id}")]
        public async Task<IActionResult> UpgradeRestaurant(Guid id)
        {
            try
            {
                await _clientService.UnfreezeRestaurantWithAdmin(Request.Headers["Authorization"], id);
                return Ok("restaurant_was_unfrozen");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("Courier/{id}")]
        public async Task<IActionResult> UpgradeCourier(Guid id)
        {
            try
            {
                await _clientService.UnfreezeCourierWithAdmin(Request.Headers["Authorization"], id);
                return Ok("courier_was_unfrozen");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllFrozenEntities()
        {
            try
            {
                var allEntities = await _clientService.GetAllFrozenEntities(Request.Headers["Authorization"]);

                if (allEntities != null)
                {
                    return Ok(allEntities);
                }

                return BadRequest("entities_null");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}