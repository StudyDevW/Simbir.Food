using FluentValidation;
using ORM_Components.DTO.CourierAPI;

namespace ORM_Components.Validators.CourierValidators
{
    public class CourierValidatorDto : AbstractValidator<CourierDto>
    {
        public CourierValidatorDto() 
        {
            RuleFor(courier => courier.userId)
                .NotEmpty().WithMessage("UserId is required.");

            RuleFor(courier => courier.car_number)
                .MaximumLength(100)
                .WithMessage("The car_number must consist of less or equal to 100 characters.")
                .When(courier => courier.car_number != null);

            RuleFor(courier => courier.status)
                .IsInEnum().WithMessage("Status must be 'IsInactive', 'IsActive'.");
        }
    }
}
