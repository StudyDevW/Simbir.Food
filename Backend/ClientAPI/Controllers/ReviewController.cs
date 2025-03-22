using ClientAPI.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Middleware_Components.Services;
using ORM_Components.DTO.ClientAPI.Review;

namespace ClientAPI.Controllers
{
    [Route("api/Reviews/")]
    [ApiController]
    public class ReviewController : ControllerBase
    {
        private readonly IClientService _clientService;
        private readonly IJwtService _jwt;
        private readonly ILogger _logger;

        public ReviewController(IClientService clientService, IJwtService jwt, ICacheService cache, IConfiguration configuration)
        {
            _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger("order-controller-logger");
            _clientService = clientService;
            _jwt = jwt;
        }

        /// <summary>
        /// Изменить данные отзыва.
        /// </summary>
        /// <returns></returns>
        [HttpPut("Update")]
        public async Task<IActionResult> UpdateReview(Guid reviewId ,[FromBody] ReviewDtoForUpdate reviewDtoForUpdate)
        {
            try
            {
                await _clientService.UpdateReview(Request.Headers["Authorization"], reviewId, reviewDtoForUpdate);
                return Ok("review_updated");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Возвращает все отзывы.
        /// </summary>
        /// <returns></returns>
        [HttpGet("Update")]
        public async Task<ActionResult<List<ReviewDto>>> GetAllReviews()
        {
            try
            {
                return await _clientService.GetAllReviews(Request.Headers["Authorization"]);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
