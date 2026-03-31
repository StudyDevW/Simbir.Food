using Microsoft.AspNetCore.Mvc;
using Middleware_Components.Services;
using ORM_Components.DTO.ClientAPI;
using VK_Components.Interfaces;
using VK_Components.Services;

namespace VK_Components.Controllers
{
    [Route("api")]
    [ApiController]
    public class VKController : ControllerBase
    {
        private readonly ICacheService _cache;
        private readonly IDatabaseOperations _databaseOperations;
        private readonly IMessageSender _messageSender;

        public VKController(IDatabaseOperations databaseOperations, ICacheService cache, IMessageSender messageSender)
        {
            _cache = cache;
            _databaseOperations = databaseOperations;
            _messageSender = messageSender;
        }

        [HttpGet("confirm")]
        public async Task<IActionResult> Confirm([FromQuery] long user_id)
        {
            if (!_cache.CheckExistKeysStorage<AuthAddUser>($"register_request_{user_id}"))
            {
                return BadRequest("request_expired");
            }

            var dtoCached = _cache.GetKeyFromStorage<AuthAddUser>($"register_request_{user_id}");

            await _databaseOperations.AddUserFromVK(dtoCached);

            _cache.DeleteKeyFromStorage($"register_request_{user_id}");

            await _messageSender.Send(user_id.ToString(), "Регистрация в сервисе успешна");

            return Ok("success");
        }
    }
}
