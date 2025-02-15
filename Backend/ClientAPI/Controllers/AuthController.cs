using ClientAPI.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Middleware_Components.Services;
using ORM_Components.DTO.ClientAPI;
using System.ComponentModel.DataAnnotations;

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
            _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger("ClientAPI | controller-logger");
            _clientService = clientService;
            _jwt = jwt;
        }

        [HttpPost("SignUp")]
        public async Task<IActionResult> UserSignUp([FromBody] AuthSignUp dtoObj)
        {
            try
            {
                var registerInfo = await _clientService.RegisterUser(dtoObj);

                if (registerInfo != null)
                {
                    return Ok(registerInfo);
                }

                return BadRequest("account_created_but_noauth");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost("SignIn")]
        public IActionResult UserSignIn([FromBody] AuthSignIn dtoObj)
        {
            try
            {
                var loginInfo = _clientService.LoginClient(dtoObj);

                if (loginInfo != null)
                {
                    return Ok(loginInfo);
                }
                else
                {
                    return NotFound();
                }
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
    }
}
