using CourierAPI.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Middleware_Components.CustomAttributes;
using ORM_Components.DTO.CourierAPI;

namespace CourierAPI.Controllers
{
    [Authorize]
    [ApiController]
    [ValidateJwt]
    [Route("api/courier/")]
    public class CourierController : ControllerBase
    {
        private readonly ICourierService _courierService;

        public CourierController(ICourierService courierService)
        {
            _courierService = courierService;
        }

        /// <summary>
        /// Получает список заказов, доступных курьеру для доставки.
        /// </summary>
        /// <returns>Список заказов для курьера.</returns>
        /// <response code="200">Возвращает список заказов</response>
        [HttpGet("ordersForCourier")]
        public async Task<ActionResult<List<OrderForCourierDto>>> GetOrderList()
        {
            return await _courierService.GetOrders();
        }

        /// <summary>
        /// Получает список активных заказов, которые этот курьер доставляет.
        /// </summary>
        /// <returns>Список заказов курьера.</returns>
        /// <response code="200">Возвращает список заказов</response>
        [HttpGet("ordersForCourierInActiveDelivery")]
        public async Task<ActionResult<List<OrderForCourierDto>>> GetActiveOrderList()
        {
            return await _courierService.GetActiveOrderList();
        }

        /// <summary>
        /// Принимает заказ курьером в доставку. Статус заказа меняется на 'Принято в доставку'.
        /// </summary>
        /// <param name="orderId">ID заказа</param>
        /// <returns>Нет содержимого</returns>
        /// <response code="204">Заказ успешно принят</response>
        [HttpPost("{orderId}/accept")]
        public async Task<IActionResult> AcceptOrder(Guid orderId)
        {
            await _courierService.AcceptOrder(orderId);
            return NoContent();
        }

        /// <summary>
        /// Меняет статус заказа на 'Курьер на месте'.
        /// </summary>
        /// <param name="orderId">ID заказа</param>
        /// <returns>Нет содержимого</returns>
        [HttpPost("{orderId}/courierOnPlace")]
        public async Task<IActionResult> CourierOnPlace(Guid orderId)
        {
            await _courierService.CourierOnPlace(orderId);
            return NoContent();
        }

        /// <summary>
        /// Меняет статус заказа на 'Заказ доставлен'.
        /// </summary>
        /// <param name="orderId">ID заказа</param>
        /// <returns>Нет содержимого</returns>
        [HttpPost("{orderId}/delivered")]
        public async Task<IActionResult> Delivered(Guid orderId)
        {
            await _courierService.OrderDelivered(orderId);
            return NoContent();
        }

        /// <summary>
        /// Получает информацию для авторизованного курьера.
        /// </summary>
        /// <returns>Информация о курьере</returns>
        [HttpGet("GetCourier")]
        public async Task<ActionResult<CourierDto>> GetAsync()
        {
            return await _courierService.GetAsync();
        }

        /// <summary>
        /// Получает информацию о всех зарегистрированных курьерах. (Develop-Method)
        /// </summary>
        /// <returns>Информация обо всех курьерах</returns>
        [HttpGet("GetAllCouriers")]
        public async Task<ActionResult<List<CourierDto>>> GetAllAsync()
        {
            return await _courierService.GetAllAsync();
        }

        /// <summary>
        /// Создание заказа вручную. (Develop-Method)
        /// </summary>
        /// <param name="courierDtoForCreate">DTO с данными нового курьера.</param>
        /// <returns>Нет содержимого</returns>
        [HttpPost("create")]
        public async Task<IActionResult> CreateAsync([FromBody] CourierDtoForCreate courierDtoForCreate)
        {
            await _courierService.CreateAsync(courierDtoForCreate);
            return NoContent();
        }

        /// <summary>
        /// Обновляет информацию курьера.
        /// </summary>
        /// <param name="courierDtoForUpdate">DTO для обновления данных курьера.</param>
        /// <returns>Нет содержимого</returns>
        [HttpPut("update")]
        public async Task<IActionResult> UpdateAsync([FromBody] CourierDtoForUpdate courierDtoForUpdate)
        {
            await _courierService.UpdateAsync(courierDtoForUpdate);
            return NoContent();
        }

        /// <summary>
        /// Удаляет курьера.
        /// </summary>
        /// <param name="courierId">ID курьера.</param>
        /// <returns>Нет содержимого</returns>
        [HttpDelete("{courierId}/delete")]
        public async Task<IActionResult> DeleteAsync(Guid courierId)
        {
            await _courierService.DeleteAsync(courierId);
            return NoContent();
        }

    }
}