using ClientAPI.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Middleware_Components.Services;
using ORM_Components.DTO.ClientAPI;

namespace ClientAPI.Controllers
{
    [Route("api/Clients/")]
    [ApiController]
    public class ClientsController : ControllerBase
    {

        private readonly IClientService _clientService;
        private readonly IJwtService _jwt;
        private readonly ILogger _logger;

        public ClientsController(IClientService clientService, IJwtService jwt, ICacheService cache, IConfiguration configuration)
        {
            _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger("clients-controller-logger");
            _clientService = clientService;
            _jwt = jwt;
        }

        [Authorize(AuthenticationSchemes = "Asymmetric")]
        [HttpGet("Me")]
        public async Task<IActionResult> GetProfileOfUser()
        {
            var profileInfo = await _clientService.ClientMeInfo(Request.Headers["Authorization"]);

            if (profileInfo != null)
            {
                return Ok(profileInfo);
            }

            return Unauthorized();
        }

        [Authorize(AuthenticationSchemes = "Asymmetric")]
        [HttpPut("Update")]
        public async Task<IActionResult> UpdateProfileOfUser([FromBody] ClientUpdate dtoObj)
        {
            try
            {
                await _clientService.UpdateClientInfo(Request.Headers["Authorization"], dtoObj);
                return Ok("account_updated");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = "Asymmetric")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProfileOfUserWithAdmin([FromBody] ClientUpdate_Admin dtoObj, Guid id)
        {
            try
            {
                await _clientService.UpdateClientInfoWithAdmin(Request.Headers["Authorization"], dtoObj, id);
                return Ok("account_updated");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = "Asymmetric")]
        [HttpGet]
        public async Task<IActionResult> GetAllProfileUsers([FromQuery] int from, [FromQuery] int count)
        {
            try
            {
                var clientsInfo = await _clientService.AllProfilesGet(Request.Headers["Authorization"], from, count);
                return Ok(clientsInfo);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = "Asymmetric")]
        [HttpPost]
        public async Task<IActionResult> CreateUserWithAdmin([FromBody] ClientAdd_Admin dtoObj)
        {
            try
            {
                await _clientService.CreateClientWithAdmin(Request.Headers["Authorization"], dtoObj);
                return Ok("account_created");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(AuthenticationSchemes = "Asymmetric")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserWithAdmin(Guid id)
        {
            try
            {
                await _clientService.DeleteClientWithAdmin(Request.Headers["Authorization"], id);
                return Ok("account_deleted");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
