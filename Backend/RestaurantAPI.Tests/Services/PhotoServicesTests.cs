using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using ORM_Components.DTO.RestaurantAPI;
using ORM_Components.Tables.Helpers;
using RestaurantAPI.Interface;
using RestaurantAPI.Model.Interface;
using RestaurantAPI.Model.Services;
using TestsBaseLib.Base;
using TestsBaseLib.Mocks;

namespace RestaurantAPI.Tests.Services;

public class PhotoServicesTests : UnitTest
{
    private readonly IPhotoServices _sut;

    public PhotoServicesTests()
    {
        var fileSystem = new InnerFileSystemHandlerMock();
        _sut = new PhotoServices(_context.Object, fileSystem);
    }

    [Fact]
    public async Task AddPhotoRestaurant_WithCorrectData_CreatesFileAndSetsImagePath()
    {
        // arrange
        var rest = Generator.GenerateRestaurant(Guid.NewGuid(), RestaurantStatus.Verified);
        itemsSetup(x => x.restaurantTable).AddItem(rest);

        var file = new Mock<IFormFile>();
        file.Setup(x => x.FileName).Returns("Restaurant1.jpg");
        file.Setup(x => x.Length).Returns(1024);

        var dto = new PhotoDTO_Restaurant
        {
            restaurantId = rest.Id,
            File = file.Object
        };

        // act
        await _sut.AddPhotoRestaurant(dto);

        // assert
        var expected = "VirtualDisk:\\Photos\\Restaurant1.jpg";
        rest.imagePath.Should().Be(expected);
    }

    [Fact]
    public async Task AddPhotoRestaurant_WithNonExistentRestaurant_ThrowsException()
    {
        // arrange
        itemsSetup(x => x.restaurantTable);

        var file = new Mock<IFormFile>();
        file.Setup(x => x.FileName).Returns("Restaurant1.jpg");
        file.Setup(x => x.Length).Returns(1024);

        var dto = new PhotoDTO_Restaurant
        {
            restaurantId = Guid.NewGuid(),
            File = file.Object
        };

        // act
        Func<Task> act = async () => await _sut.AddPhotoRestaurant(dto);

        // assert
        await act.Should().ThrowAsync<Exception>().WithMessage("restaurant_not_found");
    }

    [Fact]
    public async Task AddPhotoRestaurant_WithIncorrectFile_ThrowsException()
    {
        // arrange
        var rest = Generator.GenerateRestaurant(Guid.NewGuid(), RestaurantStatus.Verified);
        itemsSetup(x => x.restaurantTable).AddItem(rest);

        var file = new Mock<IFormFile>();
        file.Setup(x => x.FileName).Returns("Restaurant1.jpg");

        var dto = new PhotoDTO_Restaurant
        {
            restaurantId = rest.Id,
            File = file.Object
        };

        // act
        Func<Task> act = async () => await _sut.AddPhotoRestaurant(dto);

        // assert
        await act.Should().ThrowAsync<Exception>().WithMessage("file_incorrect");
    }

    [Fact]
    public async Task AddPhotoRestaurantFoodItem_WithCorrectData_CreatesFileAndSetsImagePath()
    {
        // arrange
        var food = Generator.GenerateFoodItem(Guid.NewGuid());
        itemsSetup(x => x.restaurantFoodItemsTable).AddItem(food);

        var file = new Mock<IFormFile>();
        file.Setup(x => x.FileName).Returns("Food1.jpg");
        file.Setup(x => x.Length).Returns(1024);

        var dto = new PhotoDTO_FoodItem
        {
            fooditemId = food.Id,
            File = file.Object
        };

        // act
        await _sut.AddPhotoRestaurantFoodItem(dto);

        // assert
        var expected = "VirtualDisk:\\Photos\\Food1.jpg";
        food.image.Should().Be(expected);
    }

    [Fact]
    public async Task AddPhotoRestaurantFoodItem_WithNonExistentFoodItem_ThrowsException()
    {
        // arrange
        itemsSetup(x => x.restaurantFoodItemsTable);

        var file = new Mock<IFormFile>();
        file.Setup(x => x.FileName).Returns("Food1.jpg");
        file.Setup(x => x.Length).Returns(1024);

        var dto = new PhotoDTO_FoodItem
        {
            fooditemId = Guid.NewGuid(),
            File = file.Object
        };

        // act
        Func<Task> act = async () => await _sut.AddPhotoRestaurantFoodItem(dto);

        // assert
        await act.Should().ThrowAsync<Exception>().WithMessage("fooditem_not_found");
    }

    [Fact]
    public async Task AddPhotoRestaurantFoodItem_WithIncorrectFile_ThrowsException()
    {
        // arrange
        var food = Generator.GenerateFoodItem(Guid.NewGuid());
        itemsSetup(x => x.restaurantFoodItemsTable).AddItem(food);

        var dto = new PhotoDTO_FoodItem
        {
            fooditemId = food.Id,
            File = null
        };

        // act
        Func<Task> act = async () => await _sut.AddPhotoRestaurantFoodItem(dto);

        // assert
        await act.Should().ThrowAsync<Exception>().WithMessage("file_incorrect");
    }

    [Fact]
    public async Task RemovePhotoFromRestaurant_WithCorretPath_RemovesFileAndSetsImagePath()
    {
        // arrange
        var rest = Generator.GenerateRestaurant(Guid.NewGuid(), RestaurantStatus.Verified);
        rest.imagePath = "VirtualDisk:\\Photos\\Rest1.jpg";
        itemsSetup(x => x.restaurantTable).AddItem(rest);

        // act
        await _sut.RemovePhotoFromRestaurant(rest.Id);

        // assert
        rest.imagePath.Should().Be(string.Empty);
    }

    [Fact]
    public async Task RemovePhotoFromRestaurant_WithRestaurantThatDoesntContainImagePath_ThrowsException()
    {
        // arrange
        var rest = Generator.GenerateRestaurant(Guid.NewGuid(), RestaurantStatus.Verified);
        rest.imagePath = "     ";
        itemsSetup(x => x.restaurantTable).AddItem(rest);

        // act
        Func<Task> act = async () => await _sut.RemovePhotoFromRestaurant(rest.Id);

        // assert
        await act.Should().ThrowAsync<Exception>().WithMessage("restaurant_has_no_image");
    }

    [Fact]
    public async Task RemovePhotoFromFoodItem_WithCorretPath_RemovesFileAndSetsImagePath()
    {
        // arrange
        var food = Generator.GenerateFoodItem(Guid.NewGuid());
        food.image = "VirtualDisk:\\Photos\\Food1.jpg";
        itemsSetup(x => x.restaurantFoodItemsTable).AddItem(food);

        // act
        await _sut.RemovePhotoFromFoodItem(food.Id);

        // assert
        food.image.Should().Be(string.Empty);
    }

    [Fact]
    public async Task RemovePhotoFromFoodItem_WithFoodItemThatDoesntContainImagePath_ThrowsException()
    {
        // arrange
        var food = Generator.GenerateFoodItem(Guid.NewGuid());
        food.image = null;
        itemsSetup(x => x.restaurantFoodItemsTable).AddItem(food);

        // act
        Func<Task> act = async () => await _sut.RemovePhotoFromFoodItem(food.Id);

        // assert
        await act.Should().ThrowAsync<Exception>().WithMessage("fooditem_has_no_image");
    }
}
