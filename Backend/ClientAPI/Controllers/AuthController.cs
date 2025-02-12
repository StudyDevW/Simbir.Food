using ClientAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Middleware_Components.Services;
using ORM_Components.DTO.ClientAPI;

namespace ClientAPI.Controllers
{
    [Route("api/Auth/")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IDatabaseService _database;
        private readonly IJwtService _jwt;
        private readonly ICacheService _cache;
        private readonly ILogger _logger;

        public AuthController(IDatabaseService database, IJwtService jwt, ICacheService cache, IConfiguration configuration)
        {
            _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger("ClientAPI | controller-logger");
            _database = database;
            _jwt = jwt;
            _cache = cache;
        }

        [HttpPost("SignUp")]
        public async Task<IActionResult> SignUp([FromBody] AuthSignUp dtoObj)
        {
            try
            {
                await _database.RegisterUser(dtoObj);

                return Ok("account_created");
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
                var check = _database.CheckUser(dtoObj);

                if (check.CheckHasSuccess())
                {
                    //Чисто для теста буду дописывать все
                    return Ok("Успешный вход!");
                }

            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

            return BadRequest();
        }
    }
}
