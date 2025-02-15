using ClientAPI.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Middleware_Components.Services;

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

    }
}
