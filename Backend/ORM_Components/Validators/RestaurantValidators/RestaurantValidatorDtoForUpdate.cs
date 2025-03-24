using FluentValidation;
using ORM_Components.DTO.RestaurantAPI;

namespace ORM_Components.Validators.RestaurantFoodItemsValidators
{
    public class RestaurantValidatorDtoForUpdate : AbstractValidator<RestaurantUpdate_DTO>
    {
        public RestaurantValidatorDtoForUpdate()
        {
            RuleFor(restaurant => restaurant.restaurantName)
                .MinimumLength(5).WithMessage("Restaurant name must consist at least 5 symbols.")
                .MaximumLength(100).WithMessage("Restaurant name must consist at maximum 100 symbols.")
                .When(x => !string.IsNullOrEmpty(x.restaurantName));

            RuleFor(restaurant => restaurant.address)
                .MinimumLength(5).WithMessage("Address must consist at least 5 symbols.")
                .MaximumLength(100).WithMessage("Address name must consist at maximum 100 symbols.")
                .When(x => !string.IsNullOrEmpty(x.address));

            RuleFor(restaurant => restaurant.phone_number)
                .Matches(@"^(7|8)\d{10}$").WithMessage("Phone number must start with 7 or 8 and be exactly 11 digits long.")
                .When(x => !string.IsNullOrEmpty(x.phone_number));

            RuleFor(restaurant => restaurant.description)
                .MinimumLength(5).WithMessage("Description must consist at least 5 symbols.")
                .MaximumLength(100).WithMessage("Description name must consist at maximum 100 symbols.")
                .When(x => !string.IsNullOrEmpty(x.description));

            RuleFor(restaurant => restaurant.open_time)
                .Matches(@"^([01]\d|2[0-3]):[0-5]\d$").WithMessage("Time must be in HH:mm format, where HH is 00-23 and mm is 00-59.")
                .When(x => !string.IsNullOrEmpty(x.open_time));

            RuleFor(restaurant => restaurant.close_time)
                .Matches(@"^([01]\d|2[0-3]):[0-5]\d$").WithMessage("Time must be in HH:mm format, where HH is 00-23 and mm is 00-59.")
                .When(x => !string.IsNullOrEmpty(x.close_time));
        }
    }
}
