using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Middleware_Components.Services;
using Moq;
using Moq.EntityFrameworkCore;
using ORM_Components;
using ORM_Components.DTO.RestaurantAPI;
using ORM_Components.Tables;
using ORM_Components.Tables.Helpers;
using ORM_Components.Validators.RestaurantFoodItemsValidators;
using RestaurantAPI.Model.Services;
using RestaurantAPI.Utility;
using System.Xml;
using TestsBaseLib.Base;

namespace RestaurantAPI.Tests.Services;

public class RestaurantFoodItemsServicesTests : UnitTest
{
    private readonly RestaurantFoodItemsServices _sut;
    private readonly Mock<IJwtService> _jwt;

    private readonly List<RestaurantFoodItemsTable> _foods;

    public RestaurantFoodItemsServicesTests()
    {
        _jwt = new Mock<IJwtService>();

        _foods = itemsSetup(x => x.restaurantFoodItemsTable,
            add: x => x.restaurantFoodItemsTable.Add(any<RestaurantFoodItemsTable>()),
            remove: x => x.restaurantFoodItemsTable.Remove(any<RestaurantFoodItemsTable>()),
            find: x => x.restaurantFoodItemsTable.FindAsync(any<object>()),
            removeRange: x => x.restaurantFoodItemsTable.RemoveRange(any<IEnumerable<RestaurantFoodItemsTable>>()));

        _sut = new RestaurantFoodItemsServices(_context.Object, _jwt.Object, 
            new RestaurantFoodItemValidatorDtoForCreate(),
            new RestaurantFoodItemValidatorDtoForUpdate());
    }

    [Fact]
    public async Task AddRestaurantFoodItems_WithCorrectData_ReturnsSuccessResult()
    {
        // arrange
        var rest = Generator.GenerateRestaurant(Guid.NewGuid(), RestaurantStatus.Verified);
        var rests = itemsSetup(x => x.restaurantTable,
            find: x => x.restaurantTable.FindAsync(any<object>()));
        rests.Add(rest);

        var food = Generator.GenerateFoodItem(rest.Id);
        var dto = food.ToCreateDto();

        // act
        await _sut.AddRestaurantFoodItems(dto);

        // assert
        _foods.Count.Should().Be(1);

        food.Id = _foods.First().Id;
        _foods.First().Should().BeEquivalentTo(food);
    }

    [Fact]
    public async Task AddRestaurantFoodItems_WithNonExistentRestaurant_ThrowsException()
    {
        // arrange
        itemsSetup(x => x.restaurantTable,
            find: x => x.restaurantTable.FindAsync(any<object>()));

        var food = Generator.GenerateFoodItem(Guid.NewGuid());
        var dto = food.ToDto();

        // act
        Func<Task> act = async() => await _sut.AddRestaurantFoodItems(dto);

        // assert
        await act.Should().ThrowAsync<RestaurantNotFoundException>();
    }

    [Fact]
    public async Task AddRestaurantFoodItems_WithNullDto_ThrowsException()
    {
        // arrange
        RestaurantFoodItemsDtoForCreate dto = null;

        // act
        Func<Task> act = async () => await _sut.AddRestaurantFoodItems(dto);

        // assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task AddRestaurantFoodItems_WithWrongData_ThrowsException()
    {
        // arrange
        var rest = Generator.GenerateRestaurant(Guid.NewGuid(), RestaurantStatus.Verified);
        var rests = itemsSetup(x => x.restaurantTable,
            find: x => x.restaurantTable.FindAsync(any<RestaurantTable>()));
        rests.Add(rest);

        var food = Generator.GenerateFoodItem(rest.Id);
        var dto = food.ToCreateDto(0, -100, -5, "  ");

        // act
        Func<Task> act = async() => await _sut.AddRestaurantFoodItems(dto);

        // assert
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task DeleteRestaurantFoodItems_WithExistentFoodItem_ReturnsSuccess()
    {
        // arrange
        var rest = Generator.GenerateRestaurant(Guid.NewGuid(), RestaurantStatus.Verified);
        var rests = itemsSetup(x => x.restaurantTable,
            find: x => x.restaurantTable.FindAsync(any<object>()));
        rests.Add(rest);

        var food = Generator.GenerateFoodItem(rest.Id);
        _foods.Add(food);

        // act
        await _sut.DeleteRestaurantFoodItems(food.Id);

        // assert
        _foods.Count.Should().Be(0);
    }

    [Fact]
    public async Task DeleteRestaurantFoodItems_WithNonExistentFoodItem_ThrowsException()
    {
        // arrange
        var id = Guid.NewGuid();

        // act
        Func<Task> act = async() => await _sut.DeleteRestaurantFoodItems(id);

        // assert
        await act.Should().ThrowAsync<RestaurantFoodItemNotFoundException>();
    }

    [Fact]
    public async Task UpdateRestaurantFoodItems_WithCorrectData_ReturnsSuccess()
    {
        // arrange
        var rest = Generator.GenerateRestaurant(Guid.NewGuid(), RestaurantStatus.Verified);
        var rests = itemsSetup(x => x.restaurantTable,
            find: x => x.restaurantTable.FindAsync(any<object>()));
        rests.Add(rest);

        var food = Generator.GenerateFoodItem(rest.Id);
        _foods.Add(food);

        var newFood = Generator.GenerateFoodItem(rest.Id);
        var dto = newFood.ToUpdateDto();

        // act
        await _sut.UpdateRestaurantFoodItems(food.Id, dto);

        // assert
        _foods.Count.Should().Be(1);

        newFood.Id = _foods.First().Id;
        _foods.First().Should().BeEquivalentTo(newFood);
    }

    [Fact]
    public async Task UpdateRestaurantFoodItems_WithNonExistentFoodItem_ThrowsException()
    {
        // arrange
        var newFood = Generator.GenerateFoodItem(Guid.NewGuid());
        var dto = newFood.ToUpdateDto();

        // act
        Func<Task> act = async() => await _sut.UpdateRestaurantFoodItems(Guid.NewGuid(), dto);

        // assert
        await act.Should().ThrowAsync<RestaurantFoodItemNotFoundException>();
    }

    [Fact]
    public async Task UpdateRestaurantFoodItems_WithNullDto_ThrowsException()
    {
        // arrange
        RestaurantFoodItemsDtoForUpdate dto = null;

        // act
        Func<Task> act = async () => await _sut.UpdateRestaurantFoodItems(Guid.NewGuid(), dto);

        // assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task UpdateRestaurantFoodItems_WithWrongData_ThrowsException()
    {
        // arrange
        var rest = Generator.GenerateRestaurant(Guid.NewGuid(), RestaurantStatus.Verified);
        var rests = itemsSetup(x => x.restaurantTable,
            find: x => x.restaurantTable.FindAsync(any<object>()));
        rests.Add(rest);

        var food = Generator.GenerateFoodItem(rest.Id);
        _foods.Add(food);

        var newFood = Generator.GenerateFoodItem(rest.Id);
        var dto = newFood.ToUpdateDto(0, -100, -5, "   ");

        // act
        Func<Task> act = async () => await _sut.UpdateRestaurantFoodItems(food.Id, dto);

        // assert
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task DeleteAllRestaurantFoodItems_WithExistentFoodItem_ReturnsSuccess()
    {
        // arrange
        var rest = Generator.GenerateRestaurant(Guid.NewGuid(), RestaurantStatus.Verified);
        var rests = itemsSetup(x => x.restaurantTable,
            find: x => x.restaurantTable.FindAsync(any<object>()));
        rests.Add(rest);

        _foods.AddRange(Generator.GenerateFoodItems(rest.Id, 3));

        // act
        await _sut.DeleteAllRestaurantFoodItems(rest.Id);

        // assert
        _foods.Count.Should().Be(0);
    }
}
