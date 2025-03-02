using ClientAPI.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Middleware_Components.DTO.ClientAPI;
using Middleware_Components.Services;
using ORM_Components.DTO.ClientAPI;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.Xml;

namespace ClientAPI.Controllers
{
    [Route("api/Auth/")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IClientService _clientService;
        private readonly IJwtService _jwt;
        private readonly ILogger _logger;

        public AuthController(IClientService clientService, IJwtService jwt, ICacheService cache, IConfiguration configuration)
        {
            _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger("auth-controller-logger");
            _clientService = clientService;
            _jwt = jwt;
        }

        [HttpPatch("UserAuth")]
        public async Task<IActionResult> UserTelegramAuth([FromBody] AuthAddUser dtoObj)
        {
            try
            {
                var authInfo = await _clientService.UserAuth(dtoObj);

                if (authInfo != null)
                {
                    return Ok(authInfo);
                }

                return BadRequest();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("UserRegister")]
        public async Task<IActionResult> UserTelegramRegister([FromBody] AuthAddUser dtoObj)
        {
            try
            {
                var regInfo = await _clientService.UserRegister(dtoObj);

                return Ok(regInfo);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("Validate")]
        public async Task<IActionResult> ValidateToken([Required][FromHeader(Name = "accessToken")] string? token)
        {
            if (token != null)
            {
                if (token.Contains("Bearer"))
                    return BadRequest("accessToken in this method must not contain word [Bearer]");
            }

            var validation = await _jwt.AccessTokenValidation("Bearer " + token);

            if (validation.TokenHasError())
            {
                return Unauthorized();
            }
            else if (validation.TokenHasSuccess())
            {
                _logger.LogInformation($"Токен для id: {validation.token_success.Id} валид!");
                return Ok("valid");
            }

            return BadRequest();
        }

        [Authorize(AuthenticationSchemes = "Asymmetric")]
        [HttpPut("SignOut")]
        public async Task<IActionResult> UserSignOut()
        {
            var signInfo = await _clientService.ClientSignOut(Request.Headers["Authorization"]);

            if (signInfo != null)
            {
                return Ok(signInfo);
            }

            return Unauthorized();
        }

        [HttpPost("Refresh")]
        public async Task<IActionResult> UserRefreshTokens([FromBody] Auth_RefreshTokens dtoObj)
        {
            var refreshInfo = await _clientService.RefreshClientSession(dtoObj);

            if (refreshInfo != null)
            {
                return Ok(refreshInfo);
            }

            return Unauthorized();
        }
    }
}
