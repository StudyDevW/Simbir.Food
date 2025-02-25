using DotNetEnv;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Middleware_Components.Services;
using ORM_Components;
using ORM_Components.DTO.RestaurantAPI;
using ORM_Components.Tables;
using RestaurantAPI.Model.Services;
using StackExchange.Redis;
using Telegram.Bot.Requests.Abstractions;

namespace RestaurantAPI.Model.Controllers
{
    [Route("api/Order/")]
    [Authorize(AuthenticationSchemes = "Asymmetric")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly OrderServices _orderServices;
        private readonly RabbitMQServices _rabbitMqService;
        private readonly DataContext _dbcontext;
        private readonly IJwtService _jwtService;

        public OrderController(DataContext dbcontext, IJwtService jwtService, OrderServices orderServices)
        {
            _dbcontext = dbcontext;
            _jwtService = jwtService;
            _orderServices = orderServices;
        }
        private bool IsRestaurantOpen(RestaurantTable restaurant)
        {
            var currentTime = DateTime.UtcNow;
            var openTime = restaurant.open_time;
            var closeTime = restaurant.close_time;

            return currentTime >= openTime && currentTime <= closeTime;
        }

        [HttpPost]
        public IActionResult PlaceOrder([FromBody] Order_DTO order_DTO)
        {
            _orderServices.PlaceOrder(order_DTO);
            return Ok();
        }
        [HttpPost]
        [Route("OrderFromRestaurant/{id}")]
        public async Task<IActionResult> OrderFromRestaurant(Guid id)
        {
            var validation = await _jwtService.AccessTokenValidation(Request.Headers["Authorization"]);

            if (validation.TokenHasError())
            {
                return Unauthorized();
            }
            else if (validation.TokenHasSuccess())
            {
                var restaurant = await _dbcontext.restaurantTable.FindAsync(id);
                if (restaurant == null)
                {
                    return NotFound("Ресторан не найден.");
                }

                if (!IsRestaurantOpen(restaurant))
                {
                    return BadRequest("Ресторан сейчас закрыт. Заказы принимаются только в рабочие часы.");
                }

                return Ok("Заказ успешно размещён.");

            }
            return BadRequest();
            
        }       
        [HttpPost]
        [Route("CreateOrder")]
        public async Task<IActionResult> CreateOrder([FromBody] Order_DTO order_DTO)
        {
            if (order_DTO == null)
            {
                return BadRequest("Данные заказа не могут быть пустыми");
            }
            if (order_DTO.restaurant_id == Guid.Empty)
            {
                return BadRequest("Ресторан Id не может быть пустым");
            }
            if (order_DTO.client_id == Guid.Empty)
            {
                return BadRequest("Клиент Id не может быть пустым");
            }
            if(order_DTO.courier_id == Guid.Empty)
            {
                return BadRequest("Курьер Id не может быть пустым");
            }
            if(order_DTO.status == null)
            {
                return BadRequest("Статус заказа не может быть пустым");
            }
            if (order_DTO.total_price == null && order_DTO.total_price <= 0)
            {
                return BadRequest("Цена заказа не может быть пустой или быть 0");
            }
            // Логика проверки наличия ингредиентов
            bool isIngredientsAvailable = CheckIngredients(order_DTO);

            var orderMessage = new Order_DTO
            {
                //Заполнение DTO
            };

            _rabbitMqService.SendMessage(orderMessage);

            OrderTable orderTable = new OrderTable()
            {
                client_id = order_DTO.client_id,
                restaurant_id = order_DTO.restaurant_id,
                courier_id = order_DTO.courier_id,
                status = order_DTO.status,
                total_price = order_DTO.total_price,
                order_date = order_DTO.order_date,
            };

            _dbcontext.orderTable.Add(orderTable);
            await _dbcontext.SaveChangesAsync();
            return Ok("Заказ успешно создан");
        }
        private bool CheckIngredients(OrderRequest request)
        {
            // Ваша логика проверки наличия ингредиентов
            return true; // или false в зависимости от проверки
        }
        [HttpGet]
        [Route("GetOrders")]
        public async Task<IActionResult> GetOrders()
        {
            var orders = await _dbcontext.orderTable.ToListAsync();
            return Ok(orders);
        }

        [HttpGet]
        [Route("GetOrder/{id}")]
        public async Task<IActionResult> GetOrderById(Guid id)
        {
            var order = await _dbcontext.orderTable.FindAsync(id);
            if (order == null)
            {
                return NotFound("Заказ не найден.");
            }

            return Ok(order);
        }
        [HttpPut]
        [Route("UpdateOrder/{id}")]
        public async Task<IActionResult> UpdateOrder(Guid id, [FromBody] Order_DTO order_DTO)
        {
            if (order_DTO == null)
            {
                return BadRequest("Данные заказа не могут быть пустыми");
            }
            if (order_DTO.restaurant_id == Guid.Empty)
            {
                return BadRequest("Ресторан Id не может быть пустым");
            }
            if (order_DTO.client_id == Guid.Empty)
            {
                return BadRequest("Клиент Id не может быть пустым");
            }
            if (order_DTO.courier_id == Guid.Empty)
            {
                return BadRequest("Курьер Id не может быть пустым");
            }
            if (order_DTO.status == null)
            {
                return BadRequest("Статус заказа не может быть пустым");
            }
            if (order_DTO.total_price == null && order_DTO.total_price <= 0)
            {
                return BadRequest("Цена заказа не может быть пустой или быть 0");
            }
            var order = await _dbcontext.orderTable.FindAsync(id);
            if (order == null)
            {
                return NotFound("Заказ не найден.");
            }

            order.status = order_DTO.status;
            order.total_price = order_DTO.total_price;
            _dbcontext.orderTable.Update(order);
            await _dbcontext.SaveChangesAsync();
            return Ok("Заказ успешно обновлён.");
        }
        [HttpDelete]
        [Route("DeleteOrder/{id}")]
        public async Task<IActionResult> DeleteOrder(Guid id)
        {
            var order = await _dbcontext.orderTable.FindAsync(id);
            if (order == null)
            {
                return NotFound("Заказ не найден.");
            }

            _dbcontext.orderTable.Remove(order);
            await _dbcontext.SaveChangesAsync();
            return Ok("Заказ успешно удалён.");
        }
    }
}
