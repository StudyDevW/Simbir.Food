using FluentValidation;
using ORM_Components.DTO.CourierAPI;

namespace ORM_Components.Validators.CourierValidators
{
    public class CourierValidatorDtoForUpdate : AbstractValidator<CourierDtoForUpdate>
    {
        public CourierValidatorDtoForUpdate()
        {
            RuleFor(courier => courier.Id)
                .NotEmpty().WithMessage("CourierId is required.");

            RuleFor(courier => courier.car_number)
                .Matches(@"^[А-ЯA-Z]\d{3}[А-ЯA-Z]{2}\d{2,3}$").WithMessage("Car_number must have next format: A123BC77 или A123BC177.")
                .When(courier => courier.car_number != null && courier.car_number.Length >= 1);

            RuleFor(courier => courier.status)
                .IsInEnum().WithMessage("Status must be 'IsInactive', 'IsActive'.");
        }
    }
}
