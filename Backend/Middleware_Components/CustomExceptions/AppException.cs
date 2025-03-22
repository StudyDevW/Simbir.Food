namespace RestaurantAPI.Utility
{
    public class AppException : Exception
    {
        public int StatusCode { get; }
        
        public AppException(string message, int statusCode = 400) : base(message) { StatusCode = statusCode; }
    }

    public class UserNotFoundException : AppException
    {
        public UserNotFoundException(Guid userId) : base($"Пользователь {userId} не найден.", 404) { }
    }

    public class OrderNotFoundException : AppException
    {
        public OrderNotFoundException(Guid orderId) : base($"Заказ {orderId} не найден.", 404) { }
    }

    public class RestaurantNotFoundException : AppException
    {
        public RestaurantNotFoundException(Guid restaurantId) : base($"Ресторан {restaurantId} не найден.", 404) { }
    }

    public class RestaurantFoodItemNotFoundException : AppException
    {
        public RestaurantFoodItemNotFoundException(Guid foodItemId) : base($"Блюдо {foodItemId} не найдено.", 404) { }
    }

    public class CourierNotFoundException : AppException
    {
        public CourierNotFoundException(Guid courierId) : base($"Курьер {courierId} не найден.", 404) { }
    }

    public class OrderAlreadyDelivered : AppException
    {
        public OrderAlreadyDelivered(Guid orderId) : base($"Заказ {orderId} уже доставлен. Его нельзя отклонить.", 400) { }
    }

    public class OrderLimitForCourierException : AppException
    {
        public OrderLimitForCourierException(Guid courierId) : base($"Курьер {courierId} уже доставляет 3 заказа. Нельзя иметь больше 3-ёх актуальных заказов.", 400) { }
    }

    public class OrderStatusException : AppException
    {
        public OrderStatusException(Guid orderId) : base($"Заказ {orderId} имеет некорректный статус для проведения этой операции.", 400) { }
    }

    public class OrderDeliveryException : AppException
    {
        public OrderDeliveryException(Guid orderId) : base($"Заказ {orderId} не может быть помечен как доставленный, так как курьер не назначен.", 400) { }
    }
}
