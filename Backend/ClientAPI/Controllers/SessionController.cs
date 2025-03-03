using ClientAPI.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Middleware_Components.DTO.ClientAPI;
using Middleware_Components.Services;

namespace ClientAPI.Controllers
{
    [Route("api/Session/")]
    [ApiController]
    public class SessionController : ControllerBase
    {
        private readonly IJwtService _jwt;
        private readonly ILogger _logger;
        private readonly ISessionService _session;

        public SessionController(ISessionService session, IJwtService jwt, ICacheService cache, IConfiguration configuration)
        {
            _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger("session-controller-logger");
            _jwt = jwt;
            _session = session;
        }

        /// <summary>
        /// Получить спискок всех своих сессий 
        /// </summary>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = "Asymmetric")]
        [HttpGet]
        public async Task<IActionResult> CheckSessions()
        {
            string bearer_key = Request.Headers["Authorization"];

            var validation = await _jwt.AccessTokenValidation(bearer_key);

            if (validation.TokenHasError())
            {
                return Unauthorized();
            }
            else if (validation.TokenHasSuccess())
            {
                return Ok(_session.GetSessions(validation.token_success.Id));
            }

            return BadRequest();
        }

        /// <summary>
        /// Очистить список своих сессий
        /// </summary>
        /// <returns></returns>
        [Authorize(AuthenticationSchemes = "Asymmetric")]
        [HttpDelete]
        public async Task<IActionResult> ClearSessions()
        {
            string bearer_key = Request.Headers["Authorization"];

            var validation = await _jwt.AccessTokenValidation(bearer_key);

            if (validation.TokenHasError())
            {
                return Unauthorized();
            }
            else if (validation.TokenHasSuccess())
            {
                _session.DeleteSession(validation.token_success.Id);
                return Ok("all_sessions_cleared");
            }

            return BadRequest();
        }

    }
}
