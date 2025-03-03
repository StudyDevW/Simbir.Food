using Microsoft.AspNetCore.Mvc;
using ORM_Components.DTO.PaymentAPI;
using PaymentAPI.Interfaces;

namespace PaymentAPI.Controllers
{
    [Route("api/Payment/")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        /// <summary>
        /// Оплатить пополнение баланса пользователя
        /// </summary>
        /// <returns></returns>
        [HttpPost("Pay")]
        public async Task<IActionResult> PayForUserBalance([FromBody] Payment_Release dtoObj)
        {
            try
            {
                await _paymentService.Pay(dtoObj);
                return Ok("money_sended");
            }
            catch (Exception ex) { 
                return BadRequest(ex.Message);  
            }
        }
    }
}
