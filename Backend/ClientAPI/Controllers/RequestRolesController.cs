using ClientAPI.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Middleware_Components.DTO.ClientAPI;
using Middleware_Components.Services;
using ORM_Components.DTO.ClientAPI;

namespace ClientAPI.Controllers
{
   
    [Route("api/RequestRoles/")]
    [ApiController]
    public class RequestRolesController : ControllerBase
    {
        private readonly IClientService _clientService;
        private readonly IJwtService _jwt;
        private readonly ILogger _logger;

        public RequestRolesController(IClientService clientService, IJwtService jwt, ICacheService cache, IConfiguration configuration)
        {
            _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger("order-controller-logger");
            _clientService = clientService;
            _jwt = jwt;
        }


        /// <summary>
        /// Подать заявку о создании ресторана
        /// </summary>
        /// <returns></returns>
        [HttpPost("Restaurant")]
        public async Task<IActionResult> CreateRestaurantRequest([FromBody] RestaurantAddRequest dtoObj)
        {
            try
            {
                await _clientService.CreateRestaurantRequest(Request.Headers["Authorization"], dtoObj);
                return Ok("create_restaurant_requested");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        /// <summary>
        /// Подать заявку о трудоустройстве курьером
        /// </summary>
        /// <returns></returns>
        [HttpPost("Courier")]
        public async Task<IActionResult> CreateCourierRequest([FromBody] CourierAddRequest dtoObj)
        {
            try
            {
                await _clientService.CreateCourierRequest(Request.Headers["Authorization"], dtoObj.car_number, dtoObj.request_description);
                return Ok("set_courier_requested");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Принять заявку о создании ресторана
        /// </summary>
        /// <returns></returns>
        [HttpPut("Restaurant/Accept")]
        public async Task<IActionResult> AcceptRestaurantRequest([FromBody] RequestAcceptReject dtoObj) //Только администратору
        {
            try
            {
                await _clientService.AcceptRequests(Request.Headers["Authorization"], dtoObj.requestId, "restaurant");
                return Ok("restaurant_request_accepted");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Отклонить заявку о создании ресторана
        /// </summary>
        /// <returns></returns>
        [HttpPut("Restaurant/Reject")]
        public async Task<IActionResult> RejectRestaurantRequest([FromBody] RequestAcceptReject dtoObj) //Только администратору
        {
            try
            {
                await _clientService.RejectRequests(Request.Headers["Authorization"], dtoObj.requestId, "restaurant");
                return Ok("restaurant_request_rejected");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Принять заявку и взять курьера на работу
        /// </summary>
        /// <returns></returns>
        [HttpPut("Courier/Accept")]
        public async Task<IActionResult> AcceptCourierRequest([FromBody] RequestAcceptReject dtoObj) //Только администратору
        {
            try
            {
                await _clientService.AcceptRequests(Request.Headers["Authorization"], dtoObj.requestId, "courier");
                return Ok("courier_request_accepted");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Отклонить заявку о работе курьером
        /// </summary>
        /// <returns></returns>
        [HttpPut("Courier/Reject")]
        public async Task<IActionResult> RejectCourierRequest([FromBody] RequestAcceptReject dtoObj) //Только администратору
        {
            try
            {
                await _clientService.RejectRequests(Request.Headers["Authorization"], dtoObj.requestId, "courier");
                return Ok("courier_request_rejected");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Вывести все заявки, всех пользователей
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetAllRequestsForAdmin() //Только администратору
        {
            try
            {
                var requests = await _clientService.GetAllRequestsForAdmin(Request.Headers["Authorization"]);

                if (requests != null)
                    return Ok(requests);

                return BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Получить все свои заявки на создание ресторана
        /// </summary>
        /// <returns></returns>
        [HttpGet("Restaurant")]
        public async Task<IActionResult> GetAllRestaurantRequestForUser()
        {
            try
            {
                var requests = await _clientService.GetMeRequestsRestaurant(Request.Headers["Authorization"]);

                if (requests != null)
                    return Ok(requests);

                return BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Получить свою созданную заявку на работу курьером
        /// </summary>
        /// <returns></returns>
        [HttpGet("Courier")]
        public async Task<IActionResult> GetCourierRequestForUser()
        {
            try
            {
                var requests = await _clientService.GetMeRequestCourier(Request.Headers["Authorization"]);

                if (requests != null)
                    return Ok(requests);

                return BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


    }
}
