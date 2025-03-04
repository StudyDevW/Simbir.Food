using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Middleware_Components.Services;
using Moq;
using Moq.EntityFrameworkCore;
using ORM_Components;
using ORM_Components.Tables;
using ORM_Components.Tables.Helpers;
using RestaurantAPI.Model.Services;
using TestsBaseLib.Base;

namespace RestaurantAPI.Tests.Services;

public class RestaurantFoodItemsServicesTests
{
    private readonly RestaurantFoodItemsServices _sut;
    private readonly Mock<DataContext> _context;
    private readonly Mock<IJwtService> _jwt;

    private readonly List<RestaurantFoodItemsTable> _foods;

    public RestaurantFoodItemsServicesTests()
    {
        _context = new Mock<DataContext>();
        _jwt = new Mock<IJwtService>();

        _foods = new List<RestaurantFoodItemsTable>();

        _context.Setup(x => x.restaurantFoodItemsTable).ReturnsDbSet(_foods);
        _context.Setup(x => x.restaurantFoodItemsTable.Add(It.IsAny<RestaurantFoodItemsTable>()))
            .Callback<RestaurantFoodItemsTable>(x => _foods.Add(x));
        _context.Setup(x => x.restaurantFoodItemsTable.Remove(It.IsAny<RestaurantFoodItemsTable>()))
            .Callback<RestaurantFoodItemsTable>(x => _foods.Remove(x));
        _context.Setup(x => x.restaurantFoodItemsTable.FindAsync(It.IsAny<object>()))
            .ReturnsFindAsync(_foods);

        _sut = new RestaurantFoodItemsServices(_context.Object, _jwt.Object);
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
        var rest = Generator.GenerateRestaurant(Guid.NewGuid(), RestaurantStatus.Verified);
        var rests = restaurantsSetup();
        rests.Add(rest);

        var food = Generator.GenerateFoodItem(rest.Id);
        var dto = food.ToDto();

        // act
        var result = await _sut.AddRestaurantFoodItems(dto);

        //todo: fix

        // assert
        result.Should().BeOfType<OkObjectResult>();

        food.Id = _foods.First().Id;
        _foods.First().Should().BeEquivalentTo(food);
    }
}
