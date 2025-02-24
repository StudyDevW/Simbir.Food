using CourierAPI.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ORM_Components.DTO.CourierAPI;

namespace CourierAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/courier/")]
    public class CourierController : ControllerBase
    {
        private readonly ICourierService _courierService;

        public CourierController(ICourierService courierService)
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

        [HttpGet("{courierId}/get")]
        public async Task<ActionResult<CourierDto>> GetAsync(Guid courierId)
        {
            return await _courierService.GetAsync(courierId);
        }

        [HttpGet("getAll")]
        public async Task<ActionResult<List<CourierDto>>> GetAllAsync()
        {
            return await _courierService.GetAllAsync();
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateAsync([FromBody] CourierDtoForCreate courierDtoForCreate)
        {
            await _courierService.CreateAsync(courierDtoForCreate);
            return NoContent();
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateAsync([FromBody] CourierDtoForUpdate courierDtoForUpdate)
        {
            await _courierService.UpdateAsync(courierDtoForUpdate);
            return NoContent();
        }

        [HttpDelete("{courierId}/delete")]
        public async Task<IActionResult> DeleteAsync(Guid courierId)
        {
            await _courierService.DeleteAsync(courierId);
            return NoContent();
        }

        [HttpPost("TestMethod")]
        public async Task<IActionResult> TestMethod()
        {
            await _courierService.TestMethod();
            return NoContent();
        }
    }
}
