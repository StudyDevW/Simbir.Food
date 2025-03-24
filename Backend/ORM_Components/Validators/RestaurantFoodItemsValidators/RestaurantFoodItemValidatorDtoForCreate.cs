using FluentValidation;
using ORM_Components.DTO.RestaurantAPI;

namespace ORM_Components.Validators.RestaurantFoodItemsValidators
{
    public class RestaurantFoodItemValidatorDtoForCreate : AbstractValidator<RestaurantFoodItemsDtoForCreate>
    {
        public RestaurantFoodItemValidatorDtoForCreate()
        {
            RuleFor(foodItem => foodItem.restaurant_id)
                .NotEmpty().WithMessage("Restaurant_id is required.");

            RuleFor(foodItem => foodItem.name)
                .NotEmpty().WithMessage("Name is required.")
                .MinimumLength(2).WithMessage("Minimum length for name is 2 symbols.")
                .MaximumLength(100).WithMessage("Maximum length for name is 100 symbols.");

            RuleFor(foodItem => foodItem.price)
                .NotEmpty().WithMessage("Price is required.")
                .InclusiveBetween(1, 5000).WithMessage("Price of food item must be between 1 and 5000.");

            RuleFor(foodItem => foodItem.weight)
                .NotEmpty().WithMessage("Weight is required.")
                .GreaterThan(0).WithMessage("Weight must be greater than 0.")
                .LessThan(1000).WithMessage("Weight must be less than 1000");

            RuleFor(foodItem => foodItem.calories)
                .NotEmpty().WithMessage("Calories is required.")
                .GreaterThan(0).WithMessage("Calories must be greater than 0.")
                .LessThan(10000).WithMessage("Calories must be less than 10000.");
        }
    }
}
