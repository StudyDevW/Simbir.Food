

using ORM_Components.DTO.ClientAPI;

namespace RestaurantAPI.Model.Services
{
    public class OrderServices
    {
        private readonly RabbitMQService _rabbitMqService;

        public OrderServices(RabbitMqService rabbitMqService)
        {
            _rabbitMqService = rabbitMqService;
        }

        public void PlaceOrder(Order_DTO order_DTO)
        {
            // Проверка наличия ингредиентов
            if (CheckIngredients(order_DTO.ingredients))
            {
                // Отправка сообщения о начале приготовления
                _rabbitMqService.SendMessage($"Заказ {order_DTO.id} готовится для пользователя {order_DTO.client_id}");
            }
            else
            {
                // Отправка сообщения об отмене заказа
                _rabbitMqService.SendMessage($"Заказ {order_DTO.id} для пользователя {order_DTO.client_id} отменен из-за недостаточного количества ингредиентов.");
            }
        }

        private bool CheckIngredients(List<string> ingredients)
        {
            // Логика проверки наличия ингредиентов
            return ingredients.All(ingredient => /* проверка наличия */);
        }
    }
}
