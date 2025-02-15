using ClientAPI.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Middleware_Components.Services;
using ORM_Components.DTO.ClientAPI;

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
        public async Task<IActionResult> SignUp([FromBody] AuthSignUp dtoObj)
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
        public IActionResult SignIn([FromBody] AuthSignIn dtoObj)
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
            catch(Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
