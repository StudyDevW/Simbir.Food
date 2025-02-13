using CourierAPI.Service;
using Microsoft.AspNetCore.Mvc;
using ORM_Components.DTO.CourierAPI;

namespace CourierAPI.Controllers
{
    [ApiController]
    [Route("api/courier/")]
    public class CourierController : ControllerBase
    {
        private readonly CourierService _courierService;

        public CourierController(CourierService courierService)
        {
            _courierService = courierService;
        }

        [HttpGet("ordersForCourier")]
        public async Task<ActionResult<List<OrderForCourierDto>>> GetOrderList()
        {
            return await _courierService.GetOrders();
        }

        [HttpPost("{orderId}/accept")]
        public async Task<IActionResult> AcceptOrder([FromBody] OrderLinkCourierDto orderLinkCourierDto)
        {
            await _courierService.AcceptOrder(orderLinkCourierDto);
            return NoContent();
        }

        [HttpPost("{orderId}/take")]
        public async Task<IActionResult> TakeOrder(Guid orderId)
        {
            await _courierService.TakeOrder(orderId);
            return NoContent();
        }

        [HttpPost("{orderId}/courierOnPlace")]
        public async Task<IActionResult> CourierOnPlace(Guid orderId)
        {
            await _courierService.CourierOnPlace(orderId);
            return NoContent();
        }

        [HttpPost("{orderId}/delivered")]
        public async Task<IActionResult> Delivered(Guid orderId)
        {
            await _courierService.OrderDelivered(orderId);
            return NoContent();
        }
    }
}
