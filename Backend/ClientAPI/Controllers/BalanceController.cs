using ClientAPI.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Middleware_Components.Services;
using ORM_Components.DTO.PaymentAPI;

namespace ClientAPI.Controllers
{
    [Route("api/Balance/")]
    [ApiController]
    public class BalanceController : ControllerBase
    {

        private readonly IClientService _clientService;
        private readonly IJwtService _jwt;
        private readonly ILogger _logger;

        public BalanceController(IClientService clientService, IJwtService jwt, ICacheService cache, IConfiguration configuration)
        {
            _logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger("clients-controller-logger");
            _clientService = clientService;
            _jwt = jwt;
        }


        //[HttpPost("Insert")]
        //public async Task<IActionResult> InsertMoneyOnBalance()
        //{
        //    return Ok();
        //}

        /// <summary>
        /// Вывод средств с баланса
        /// </summary>
        /// <returns></returns>
        [HttpPost("Out")]
        public async Task<IActionResult> OutMoneyFromBalance([FromBody] PaymentOut dtoObj)
        {
            try
            {
                await _clientService.MoneyOut(Request.Headers["Authorization"], dtoObj);
                return Ok("money_sended");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        //[HttpGet("Cards")]
        //public async Task<IActionResult> LinkedBankCards()
        //{
        //    return Ok();
        //}


        //[HttpDelete("Card/{id}")]
        //public async Task<IActionResult> DeleteLinkedBankCard()
        //{
        //    return Ok();
        //}
    }
}
