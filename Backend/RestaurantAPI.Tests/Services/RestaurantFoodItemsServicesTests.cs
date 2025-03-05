using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Middleware_Components.Services;
using Moq;
using Moq.EntityFrameworkCore;
using ORM_Components;
using ORM_Components.DTO.RestaurantAPI;
using ORM_Components.Tables;
using ORM_Components.Tables.Helpers;
using RestaurantAPI.Model.Services;
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

        _sut = new RestaurantFoodItemsServices(_context.Object, _jwt.Object);
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
        var dto = food.ToDto();

        // act
        var result = await _sut.AddRestaurantFoodItems(dto);

        // assert
        result.Should().BeOfType<string>();
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
        await act.Should().ThrowAsync<Exception>().WithMessage("Ресторан с указанным ID не найден.");
    }

    [Fact]
    public async Task AddRestaurantFoodItems_WithNullDto_ThrowsException()
    {
        // arrange
        RestaurantFoodItems_DTO dto = null;

        // act
        Func<Task> act = async () => await _sut.AddRestaurantFoodItems(dto);

        // assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Данные блюда не могут быть пустыми.");
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
        var dto = food.ToDto();
        dto.price = 0;
        dto.weight = -100;
        dto.calories = -5;
        dto.name = "  ";

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
        var result = await _sut.DeleteRestaurantFoodItems(food.Id);

        // assert
        result.Should().BeOfType<string>();
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
        await act.Should().ThrowAsync<Exception>().WithMessage("Блюдо не найдено.");
    }

    //todo: DeleteAllRestaurantFoodItems 

    [Fact]
    public async Task PutRestaurantFoodItems_WithCorrectData_ReturnsSuccess()
    {
        // arrange
        var rest = Generator.GenerateRestaurant(Guid.NewGuid(), RestaurantStatus.Verified);
        var rests = itemsSetup(x => x.restaurantTable,
            find: x => x.restaurantTable.FindAsync(any<object>()));
        rests.Add(rest);

        var food = Generator.GenerateFoodItem(rest.Id);
        _foods.Add(food);

        var newFood = Generator.GenerateFoodItem(rest.Id);
        var dto = newFood.ToDto();

        // act
        var result = await _sut.PutRestaurantFoodItems(dto, food.Id);

        // assert
        result.Should().BeOfType<string>();
        _foods.Count.Should().Be(1);

        newFood.Id = _foods.First().Id;
        _foods.First().Should().BeEquivalentTo(newFood);
    }

    [Fact]
    public async Task PutRestaurantFoodItems_WithNonExistentFoodItem_ThrowsException()
    {
        // arrange
        var newFood = Generator.GenerateFoodItem(Guid.NewGuid());
        var dto = newFood.ToDto();

        // act
        Func<Task> act = async() => await _sut.PutRestaurantFoodItems(dto, Guid.NewGuid());

        // assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Блюдо с указанным ID не найдено.");
    }

    [Fact]
    public async Task PutRestaurantFoodItems_WithNullDto_ThrowsException()
    {
        // arrange
        RestaurantFoodItems_DTO dto = null;

        // act
        Func<Task> act = async () => await _sut.PutRestaurantFoodItems(dto, Guid.NewGuid());

        // assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Данные блюда не могут быть пустыми.");
    }

    [Fact]
    public async Task PutRestaurantFoodItems_WithWrongData_ThrowsException()
    {
        // arrange
        var rest = Generator.GenerateRestaurant(Guid.NewGuid(), RestaurantStatus.Verified);
        var rests = itemsSetup(x => x.restaurantTable,
            find: x => x.restaurantTable.FindAsync(any<object>()));
        rests.Add(rest);

        var food = Generator.GenerateFoodItem(rest.Id);
        _foods.Add(food);

        var newFood = Generator.GenerateFoodItem(rest.Id);
        var dto = newFood.ToDto();
        dto.price = 0;
        dto.weight = -100;
        dto.calories = -5;
        dto.name = "  ";

        // act
        Func<Task> act = async () => await _sut.PutRestaurantFoodItems(dto, food.Id);

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
        var result = await _sut.DeleteAllRestaurantFoodItems(rest.Id);

        // assert
        result.Should().BeOfType<string>();
        _foods.Count.Should().Be(0);
    }


    [Fact]
    public async Task DeleteAllRestaurantFoodItems_WithZeroFoodItems_ThrowsException()
    {
        // arrange
        var rest = Generator.GenerateRestaurant(Guid.NewGuid(), RestaurantStatus.Verified);
        var rests = itemsSetup(x => x.restaurantTable,
            find: x => x.restaurantTable.FindAsync(any<object>()));
        rests.Add(rest);

        // act
        Func<Task> act = async() => await _sut.DeleteAllRestaurantFoodItems(rest.Id);

        // assert
        await act.Should().ThrowAsync<Exception>("Нет доступных блюд для удаления в указанном ресторане.");
    }
}
