using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Middleware_Components.Services;
using Moq;
using ORM_Components.Tables;
using ORM_Components;
using RestaurantAPI.Model.Controllers;
using Moq.EntityFrameworkCore;
using ORM_Components.DTO.RestaurantAPI;
using Middleware_Components.JWT.DTO.Token;
using TestsBaseLib.Base;
using FluentAssertions;
using StackExchange.Redis;

namespace RestaurantAPI.Tests.Controllers;

public class RestaurantFoodItemsControllerTests
{
    private readonly RestaurantFoodItemsController _sut;
    private readonly Mock<DataContext> _context;
    private readonly Mock<IJwtService> _jwt;

    private readonly List<RestaurantFoodItemsTable> _foods;

    public RestaurantFoodItemsControllerTests()
    {
        _jwt = new Mock<IJwtService>();
        _context = new Mock<DataContext>();
        _foods = new List<RestaurantFoodItemsTable>();

        _context.Setup(x => x.restaurantFoodItemsTable).ReturnsDbSet(_foods);
        _context.Setup(x => x.restaurantFoodItemsTable.Add(It.IsAny<RestaurantFoodItemsTable>()))
            .Callback<RestaurantFoodItemsTable>(x => _foods.Add(x));
        _context.Setup(x => x.restaurantFoodItemsTable.Remove(It.IsAny<RestaurantFoodItemsTable>()))
            .Callback<RestaurantFoodItemsTable>(x => _foods.Remove(x));
        _context.Setup(x => x.restaurantFoodItemsTable.FindAsync(It.IsAny<object>()))
            .ReturnsFindAsync(_foods);

        _sut = new RestaurantFoodItemsController(_context.Object, _jwt.Object);
        _sut.ConfigureContext();
    }

    private List<RestaurantTable> restaurantsSetup()
    {
        var rests = new List<RestaurantTable>();

        _context.Setup(x => x.restaurantTable).ReturnsDbSet(rests);
        _context.Setup(x => x.restaurantTable.FindAsync(It.IsAny<object>()))
            .ReturnsFindAsync(rests);

        return rests;
    }

    [Fact]
    public async Task AddRestaurantFoodItems_WithCorrectData_ReturnsSuccessResult()
    {
        // arrange
        var rest = Generator.GenerateRestaurant(Guid.NewGuid());
        var rests = restaurantsSetup();
        rests.Add(rest);

        var food = Generator.GenerateFoodItem(rest.Id);
        var dto = food.ToDto();

        _jwt.InitJwt("98423523", "Client");

        // act
        var result = await _sut.AddRestaurantFoodItems(dto);

        // assert
        result.Should().BeOfType<OkObjectResult>();

        food.Id = _foods.First().Id;
        _foods.First().Should().BeEquivalentTo(food);
    }

    [Fact]
    public async Task AddRestaurantFoodItems_WithNullDto_ReturnsBadRequest()
    {
        // arrange
        RestaurantFoodItems_DTO dto = null;

        // act
        var result = await _sut.AddRestaurantFoodItems(dto);

        // assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task AddRestaurantFoodItems_WithWrongRestId_ReturnsBadRequest()
    {
        // arrange
        var rests = restaurantsSetup();

        var food = Generator.GenerateFoodItem(Guid.NewGuid());
        var dto = food.ToDto();

        _jwt.InitJwt("98423523", "Client");

        // act
        var result = await _sut.AddRestaurantFoodItems(dto);

        // assert
        result.Should().BeOfType<BadRequestObjectResult>();
        _foods.Count.Should().Be(0);
    }

    [Theory]
    [InlineData("", 500, 1000, 150)]
    [InlineData("Plov", 0, 1000, 150)]
    [InlineData("Plov", 500, -1000, 150)]
    [InlineData("Plov", 500, 1000, 0)]
    public async Task AddRestaurantFoodItems_WithWrongData_ReturnsBadRequest(string name, int weight, int calories, int price)
    {
        // arrange
        var rest = Generator.GenerateRestaurant(Guid.NewGuid());
        var rests = restaurantsSetup();
        rests.Add(rest);

        var food = Generator.GenerateFoodItem(rest.Id);
        var dto = food.ToDto();

        dto.name = name;
        dto.price = price;
        dto.weight = weight;
        dto.calories = calories;

        _jwt.InitJwt("98423523", "Client");

        // act
        var result = await _sut.AddRestaurantFoodItems(dto);

        // assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task DeleteRestaurantFoodItems_WithCorrectFoodId_ReturnsSuccessResult()
    {
        // arrange
        var food = Generator.GenerateFoodItem(Guid.NewGuid());
        _foods.Add(food);

        _jwt.InitJwt("98423523", "Client");

        // act
        var result = await _sut.DeleteRestaurantFoodItems(food.Id);

        // assert
        result.Should().BeOfType<OkObjectResult>();
        _foods.Count.Should().Be(0);
    }

    [Fact]
    public async Task DeleteRestaurantFoodItems_WithWrongFoodId_ReturnsNotFound()
    {
        // arrange
        var id = Guid.NewGuid();

        _jwt.InitJwt("98423523", "Client");

        // act
        var result = await _sut.DeleteRestaurantFoodItems(id);

        // assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task PutRestaurantFoodItems_WithCorrectData_ReturnsSuccessResult()
    {
        // arrange
        var rest = Guid.NewGuid();
        var food = Generator.GenerateFoodItem(rest);
        _foods.Add(food);

        var newFood = Generator.GenerateFoodItem(rest);
        var dto = newFood.ToDto();

        _jwt.InitJwt("98423523", "Client");

        // act
        var result = await _sut.PutRestaurantFoodItems(food.Id, dto);

        // assert
        result.Should().BeOfType<OkObjectResult>();

        newFood.Id = _foods.First().Id;
        _foods.First().Should().BeEquivalentTo(newFood);
    }

    [Fact]
    public async Task PutRestaurantFoodItems_WithWrongFoodId_ReturnsNotFound()
    {
        // arrange
        var newFood = Generator.GenerateFoodItem(Guid.NewGuid());
        var dto = newFood.ToDto();

        _jwt.InitJwt("98423523", "Client");

        // act
        var result = await _sut.PutRestaurantFoodItems(Guid.NewGuid(), dto);

        // assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task PutRestaurantFoodItems_WithNullDto_ReturnsBadRequest()
    {
        // arrange
        RestaurantFoodItems_DTO dto = null;

        _jwt.InitJwt("98423523", "Client");

        // act
        var result = await _sut.PutRestaurantFoodItems(Guid.NewGuid(), dto);

        // assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Theory]
    [InlineData("", 500, 1000, 150)]
    [InlineData("Plov", 0, 1000, 150)]
    [InlineData("Plov", 500, -1000, 150)]
    [InlineData("Plov", 500, 1000, 0)]
    public async Task PutRestaurantFoodItems_WithWrongData_ReturnsBadRequest(string name, int weight, int calories, int price)
    {
        // arrange
        var rest = Guid.NewGuid();
        var food = Generator.GenerateFoodItem(rest);
        _foods.Add(food);

        var newFood = Generator.GenerateFoodItem(rest);
        var dto = newFood.ToDto();

        dto.name = name;
        dto.price = price;
        dto.weight = weight;
        dto.calories = calories;

        _jwt.InitJwt("98423523", "Client");

        // act
        var result = await _sut.PutRestaurantFoodItems(food.Id, dto);

        // assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }
}
