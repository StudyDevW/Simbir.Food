using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using ORM_Components.DTO.ClientAPI;
using ORM_Components.Tables;
using ORM_Components.Tables.Helpers;
using RestaurantAPI.Model.GetRastaurant;
using RestaurantAPI.Model.Services;
using Telegram_Components.Interfaces;
using TestsBaseLib.Base;

namespace RestaurantAPI.Tests.Services;

public class RestaurantServicesTests : UnitTest
{
    private readonly RestaurantServices _sut;
    private readonly List<RestaurantTable> _rests;

    public RestaurantServicesTests()
    {
        var sender = new Mock<IMessageSender>();

        _rests = itemsSetup(x => x.restaurantTable,
            removeRange: x => x.restaurantTable.RemoveRange(any<IEnumerable<RestaurantTable>>()),
            remove: x => x.restaurantTable.Remove(any<RestaurantTable>()));

        _sut = new RestaurantServices(_context.Object, sender.Object);
    }

    [Fact]
    public async Task CreateRestaurant_WithExistentRestaurant_AcceptsRestaurantCreationRequest()
    {
        // arrange
        var rest = Generator.GenerateRestaurant(Guid.NewGuid(), RestaurantStatus.Unverified);
        _rests.Add(rest);

        // act
        await _sut.CreateRestaurant(rest.Id);

        // assert
        rest.status.Should().Be(RestaurantStatus.Verified);
    }

    [Fact]
    public async Task CreateRestaurant_WithNonExistentRestaurant_ThrowsException()
    {
        // arrange
        var id = Guid.NewGuid();

        // act
        Func<Task> act = async() => await _sut.CreateRestaurant(id);

        // assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Ресторан не найден.");
    }

    [Fact]
    public async Task OrderRejections_WithExistentOrder_RejectsOrder()
    {
        // arrange
        var orders = itemsSetup(x => x.orderTable,
            find: x => x.orderTable.FindAsync(any<object>()));
        var order = Generator.GenerateOrder(OrderStatus.Accepted);
        orders.Add(order);

        var users = itemsSetup(x => x.userTable,
            find: x => x.userTable.FindAsync(any<object>()));
        var user = Generator.GenerateUser();
        users.Add(user);

        var dto = new Order_DTO
        {
            id = order.Id,
            client_id = user.Id
        };

        // act
        await _sut.OrderRejections(dto);

        // assert
        order.status.Should().Be(OrderStatus.Denied);
    }

    [Fact]
    public async Task OrderRejections_WithNonExistentOrder_ThrowsException()
    {
        // arrange
        var orders = itemsSetup(x => x.orderTable,
            find: x => x.orderTable.FindAsync(any<object>()));

        var users = itemsSetup(x => x.userTable,
            find: x => x.userTable.FindAsync(any<object>()));
        var user = Generator.GenerateUser();
        users.Add(user);

        var dto = new Order_DTO
        {
            id = Guid.NewGuid(),
            client_id = user.Id
        };

        // act
        Func<Task> act = async() => await _sut.OrderRejections(dto);

        // assert
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task DeleteRestaurant_WithExistentRestaurant_DeletesRestaurant()
    {
        // arrange
        var rest = Generator.GenerateRestaurant(Guid.NewGuid(), RestaurantStatus.Unverified);
        _rests.Add(rest);

        // act
        await _sut.DeleteRestaurant(rest.Id);

        // assert
        _rests.Count.Should().Be(0);
    }

    [Fact]
    public async Task DeleteRestaurant_WithNonExistentRestaurant_ThrowsException()
    {
        // arrange
        var id = Guid.NewGuid();

        // act
        Func<Task> act = async() => await _sut.DeleteRestaurant(id);

        // assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Ресторан не найден.");
    }
}
