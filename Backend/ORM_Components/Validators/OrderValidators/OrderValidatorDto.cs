using FluentValidation;
using ORM_Components.DTO.RestaurantAPI;

namespace ORM_Components.Validators.OrderValidators
{
    public class OrderValidatorDto : AbstractValidator<Order_DTO>
    {
        public OrderValidatorDto()
        {
            RuleFor(order => order.id)
                .NotEmpty().WithMessage("OrderId is required.");

            RuleFor(order => order.client_id)
                .NotEmpty().WithMessage("ClientId is required.");

            RuleFor(order => order.restaurant_id)
                .NotEmpty().WithMessage("Restaurant_id.");

            RuleFor(order => order.status)
                .IsInEnum().WithMessage("OrderStatus must be 'WaitingForPay', 'Accepted', " +
                "'Denied', 'Ready', 'WaitingForDelivery', 'CourierOnPlace', 'Delivered'.");

            RuleFor(order => order.total_price)
                .NotEmpty().WithMessage("Total_price is required.")
                .GreaterThanOrEqualTo(0).WithMessage("Total_price can't be less than 0 (zero).");

            RuleFor(order => order.order_date)
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Дата заказа не может быть в будущем.");
        }
    }
}
