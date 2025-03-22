using FluentValidation;
using ORM_Components.DTO.RestaurantAPI;

namespace ORM_Components.Validators.RestaurantFoodItemsValidators
{
    public class RestaurantFoodItemValidatorDtoForUpdate : AbstractValidator<RestaurantFoodItemsDtoForUpdate>
    {
        public RestaurantFoodItemValidatorDtoForUpdate()
        {
            RuleFor(foodItem => foodItem.name)
                .MinimumLength(2).WithMessage("Minimum length for name is 2 symbols.")
                .MaximumLength(100).WithMessage("Maximum length for name is 100 symbols.")
                .When(x => !string.IsNullOrEmpty(x.name));

            RuleFor(foodItem => foodItem.price)
                .InclusiveBetween(1, 5000).WithMessage("Price of food item must be between 1 and 5000.")
                .When(x => x.price.HasValue);

            RuleFor(foodItem => foodItem.weight)
                .GreaterThan(0).WithMessage("Weight must be greater than 0.")
                .LessThan(1000).WithMessage("Weight must be less than 1000.")
                .When(x => x.weight.HasValue);

            RuleFor(foodItem => foodItem.calories)
                .GreaterThan(0).WithMessage("Calories must be greater than 0.")
                .LessThan(10000).WithMessage("Calories must be less than 10000.")
                .When(x => x.calories.HasValue);
        }
    }
}
