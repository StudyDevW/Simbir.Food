using FluentValidation;
using FluentValidation.TestHelper;
using ORM_Components.DTO.CourierAPI;
using ORM_Components.Tables.Helpers;
using ORM_Components.Validators.CourierValidators;

namespace CourierAPI.Tests.Validation;

public class CourierValidationTests
{
    private readonly IValidator<CourierDtoForCreate> _createValidator;
    private readonly IValidator<CourierDtoForUpdate> _updateValidator;

    public CourierValidationTests()
    {
        _createValidator = new CourierValidatorDtoForCreate();
        _updateValidator = new CourierValidatorDtoForUpdate();
    }

    [Theory]
    [InlineData("c470a5bf-424f-47fd-b8e4-b50c429cb8b5", null)]
    [InlineData("c470a5bf-424f-47fd-b8e4-b50c429cb8b5", "A859BC73")]
    [InlineData("c470a5bf-424f-47fd-b8e4-b50c429cb8b5", "A290BC173")]
    public void CourierDtoForCreateValidation_WithCorrectData_ReturnsValid(Guid id, string? carNumber)
    {
        // arrange
        var dto = new CourierDtoForCreate(id, carNumber);

        // act
        var result = _createValidator.TestValidate(dto);

        // assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("F88G8G53")]
    [InlineData("5AAA2473")]
    [InlineData("526FV")]
    public void CourierDtoForCreateValidation_WithWrongCarNumber_ReturnsInvalid(string? carNumber)
    {
        // arrange
        var dto = new CourierDtoForCreate(Guid.NewGuid(), carNumber);

        // act
        var result = _createValidator.TestValidate(dto);

        // assert
        result.ShouldHaveValidationErrorFor(nameof(CourierDtoForCreate.car_number));
    }

    [Fact]
    public void CourierDtoForCreateValidation_WithWrongUserId_ReturnsInvalid()
    {
        // arrange
        var dto = new CourierDtoForCreate(Guid.Empty, null);

        // act
        var result = _createValidator.TestValidate(dto);

        // assert
        result.ShouldHaveValidationErrorFor(nameof(CourierDtoForCreate.userId));
    }
}
