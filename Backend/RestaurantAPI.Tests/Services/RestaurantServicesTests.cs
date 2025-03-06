using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using ORM_Components.DTO.ClientAPI;
using ORM_Components.DTO.RestaurantAPI;
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

    private readonly Mock<IMessageSender> _sender;

    public RestaurantServicesTests()
    {
        _sender = new Mock<IMessageSender>();

        _rests = itemsSetup(x => x.restaurantTable,
            removeRange: x => x.restaurantTable.RemoveRange(any<IEnumerable<RestaurantTable>>()),
            remove: x => x.restaurantTable.Remove(any<RestaurantTable>()));

        _sut = new RestaurantServices(_context.Object, _sender.Object);
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

    [Fact]
    public async Task GetRestaurant_WithExistentRestaurant_ReturnsRestaurant()
    {
        // arrange
        var rest = Generator.GenerateRestaurant(Guid.NewGuid(), RestaurantStatus.Unverified);
        _rests.Add(rest);

        // act
        var result = await _sut.GetRestaurant(rest.Id);

        // assert
        result.Should().NotBeNull();
        result.Id.Should().Be(rest.Id);
    }

    [Fact]
    public async Task GetRestaurant_WithNonExistentRestaurant_ThrowsException()
    {
        // arrange
        var id = Guid.NewGuid();

        // act
        Func<Task> act = async() => await _sut.GetRestaurant(id);

        // assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Ресторан не найден.");
    }

    [Fact]
    public async Task UpdateRestaurant_WithCorrectData_UpdatesRestaurant()
    {
        // arrange
        var userId = Guid.NewGuid();

        var rest = Generator.GenerateRestaurant(userId, RestaurantStatus.Verified);
        _rests.Add(rest);

        var newRest = Generator.GenerateRestaurant(userId, RestaurantStatus.Verified);
        var dto = newRest.ToUpdateDto();

        // act
        await _sut.UpdateRestaurant(rest.Id, dto);

        // arrange
        newRest.Id = rest.Id;
        rest.Should().BeEquivalentTo(newRest);
    }

    [Fact]
    public async Task UpdateRestaurant_WithNullData_DoesntUpdateRestaurant()
    {
        // arrange
        var userId = Guid.NewGuid();

        var rest = Generator.GenerateRestaurant(userId, RestaurantStatus.Verified);
        _rests.Add(rest);

        var dto = new RestaurantUpdate_DTO(null, null, null, null, null, null, null, null);

        // act
        await _sut.UpdateRestaurant(rest.Id, dto);

        // arrange
        rest.restaurantName.Should().NotBeNull();
        rest.address.Should().NotBeNull();
        rest.phone_number.Should().NotBeNull();
        rest.description.Should().NotBeNull();
        rest.imagePath.Should().NotBeNull();
        rest.open_time.Should().NotBeNull();
        rest.close_time.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateRestaurant_WithNonExistentRestaurant_ThrowsException()
    {
        // arrange
        var userId = Guid.NewGuid();

        var newRest = Generator.GenerateRestaurant(userId, RestaurantStatus.Verified);
        var dto = newRest.ToUpdateDto();

        // act
        Func<Task> act = async () => await _sut.UpdateRestaurant(Guid.NewGuid(), dto);

        // arrange
        await act.Should().ThrowAsync<Exception>().WithMessage("Ресторан не найден.");
    }

    [Fact]
    public async Task GetRestaurantMark_WithSomeRestaurantsAndReviews_ReturnsRestaurantsMarks()
    {
        // arrange
        var rests = Generator.GenerateRestaurants(Guid.NewGuid(), 3);
        _rests.AddRange(rests);

        itemsSetup(x => x.reviewTable).AddRange(new List<ReviewTable>
        {
            Generator.GenReview(rests[0].Id, 3),
            Generator.GenReview(rests[1].Id, 2),
            Generator.GenReview(rests[1].Id, 5),
            Generator.GenReview(rests[2].Id, 4),
            Generator.GenReview(rests[2].Id, 4)
        });

        // act
        var result = await _sut.GetRestaurantMark();

        // assert
        result.Should().NotBeNull();
        result[0].Id.Should().Be(rests[0].Id);
        result[0].averageMark.Should().Be(3);

        result[1].Id.Should().Be(rests[1].Id);
        result[1].averageMark.Should().Be(3);

        result[2].Id.Should().Be(rests[2].Id);
        result[2].averageMark.Should().Be(4);
    }

    [Fact]
    public async Task SetReadyStatusForOrder_WithCorrectData_SetsOrderStatusToReady()
    {
        // arrange
        var client = Generator.GenerateUser();
        var users = itemsSetup(x => x.userTable);
        users.Add(client);

        var order = Generator.GenerateOrder(client.Id, Guid.NewGuid(), OrderStatus.Accepted);
        itemsSetup(x => x.orderTable).Add(order);

        users.AddRange(new List<UserTable>
        {
            Generator.GenerateUser(),
            Generator.GenerateUser(),
            Generator.GenerateUser()
        });

        itemsSetup(x => x.courierTable).AddRange(new List<CourierTable>
        {
            Generator.GenerateCourier(users[0].Id, CourierStatus.IsActive),
            Generator.GenerateCourier(users[1].Id, CourierStatus.IsActive),
            Generator.GenerateCourier(users[2].Id, CourierStatus.IsInactive)
        });

        // act
        await _sut.SetReadyStatusForOrder(order.Id);

        // assert
        order.status.Should().Be(OrderStatus.Ready);

        _sender.Verify(x => x.Send(users[0].telegram_chat_id.ToString(), any<string>()));
        _sender.Verify(x => x.Send(users[1].telegram_chat_id.ToString(), any<string>()));
    }
}
