using CourierAPI.Service;
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Middleware_Components.Broker;
using Moq;
using Moq.EntityFrameworkCore;
using ORM_Components;
using ORM_Components.DTO.CourierAPI;
using ORM_Components.Interfaces;
using ORM_Components.Tables;
using ORM_Components.Tables.Helpers;
using ORM_Components.Validators.CourierValidators;
using System.Security.Claims;
using Telegram.Bot.Types;
using Telegram_Components.Interfaces;
using TestsBaseLib.Base;

namespace CourierAPI.Tests.Services;

public class CourierServiceTests : UnitTest
{
    private readonly Mock<IRabbitMQService> _rabbit;
    private readonly Mock<IMessageSender> _sender;
    private readonly CourierService _sut;
    private readonly Mock<IHttpContextAccessor> _http;

    public CourierServiceTests()
    {
        _sender = new Mock<IMessageSender>();
        _rabbit = new Mock<IRabbitMQService>();
        var mail = new Mock<IMailSender>();
        _http = new Mock<IHttpContextAccessor>();

        _sut = new CourierService(_context.Object, _sender.Object, _rabbit.Object, mail.Object,
            Mock.Of<IValidator<CourierDtoForCreate>>(),
            Mock.Of<IValidator<CourierDtoForUpdate>>(),
            _http.Object);
    }

    private void setUser(Guid id)
    {
        _http.Setup(x => x.HttpContext.User.FindFirst("Id")).Returns(new Claim("UserId", id.ToString()));
    }

    [Fact]
    public async Task AcceptOrder_WithCorrectData_SetCourierOfAnOrder()
    {
        // arrange
        var client = Generator.GenerateUser();
        var order = Generator.GenerateOrder(client.Id, Guid.NewGuid(), OrderStatus.Ready);

        var user = Generator.GenerateUser();
        var courier = Generator.GenerateCourier(user.Id);

        var users = itemsSetup(x => x.userTable);
        users.Add(user);
        users.Add(client);

        itemsSetup(x => x.orderHistory);
        itemsSetup(x => x.orderTable).Add(order);
        itemsSetup(x => x.courierTable).Add(courier);

        setUser(user.Id);

        // act
        await _sut.AcceptOrder(order.Id);

        // asset
        order.courier_id.Should().Be(courier.Id);
    }

    [Fact]
    public async Task AcceptOrder_WithWrongOrderId_ThrowsOrderException()
    {
        // arrange
        itemsSetup(x => x.orderTable);
        itemsSetup(x => x.courierTable);

        setUser(Guid.NewGuid());

        // act
        Func<Task> act = async () => await _sut.AcceptOrder(Guid.NewGuid());

        // assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Заказ не найден.");
    }

    [Fact]
    public async Task AcceptOrder_WithWrongCourierId_ThrowsCourierException()
    {
        // arrange
        var client = Generator.GenerateUser();
        var order = Generator.GenerateOrder(client.Id, Guid.NewGuid(), OrderStatus.Ready);

        var user = Generator.GenerateUser();
        var courier = Generator.GenerateCourier(user.Id);

        var users = itemsSetup(x => x.userTable);
        users.Add(user);
        users.Add(client);

        itemsSetup(x => x.orderHistory);
        itemsSetup(x => x.orderTable).Add(order);
        itemsSetup(x => x.courierTable);

        setUser(user.Id);

        // act
        Func<Task> act = async () => await _sut.AcceptOrder(order.Id);

        // assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Курьер не найден.");
    }

    //[Fact]
    //public async Task TakeOrder_WithStatusReady_SetOrderStatusToWaitingForDelivery()
    //{
    //    // arrange
    //    var owner = Generator.GenerateUser();
    //    var order = Generator.GenerateOrder(owner.Id, Guid.NewGuid(), OrderStatus.Ready);

    //    var history = new List<OrderStatusHistoryTable>();

    //    _context.Setup(x => x.orderTable).ReturnsDbSet(new List<OrderTable> { order });
    //    _context.Setup(x => x.orderHistory).ReturnsDbSet(history);
    //    _context.Setup(x => x.userTable).ReturnsDbSet(new List<UserTable> { owner });
    //    _context.Setup(x => x.orderHistory.Add(It.IsAny<OrderStatusHistoryTable>()))
    //        .Callback<OrderStatusHistoryTable>(x => history.Add(x));

        

    //    // act
    //    await _sut.TakeOrder(order.Id);

    //    // assert
    //    order.status.Should().Be(OrderStatus.WaitingForDelivery);
    //    history.First().status.Should().Be(OrderStatus.WaitingForDelivery);
    //    history.First().order_id.Should().Be(order.Id);
    //}

    //[Fact]
    //public async Task TakeOrder_WithNotExpectedStatus_ThrowsException()
    //{
    //    // arrange
    //    var order = Generator.GenerateOrder(OrderStatus.WaitingForDelivery);

    //    _context.Setup(x => x.orderTable).ReturnsDbSet(new List<OrderTable> { order });

        

    //    // act
    //    Func<Task> act = async() => await _sut.TakeOrder(order.Id);

    //    // assert
    //    await act.Should().ThrowAsync<Exception>().WithMessage("Статус не соответствует ожидаемому.");
    //}

    //[Fact]
    //public async Task TakeOrder_WithWrongOrderId_ThrowsException()
    //{
    //    // arrange
    //    var order = Generator.GenerateOrder(OrderStatus.Ready);

    //    _context.Setup(x => x.orderTable).ReturnsDbSet(new List<OrderTable> { order });

        

    //    // act
    //    Func<Task> act = async () => await _sut.TakeOrder(Guid.NewGuid());

    //    // assert
    //    await act.Should().ThrowAsync<Exception>().WithMessage("Заказ не найден.");
    //}

    [Fact]
    public async Task CourierOnPlace_WithStatusWaitingForDelivery_SetOrderStatusToCourierOnPlace()
    {
        // arrange
        var owner = Generator.GenerateUser();
        var order = Generator.GenerateOrder(owner.Id, Guid.NewGuid(), OrderStatus.WaitingForDelivery);

        var history = new List<OrderStatusHistoryTable>();

        _context.Setup(x => x.orderTable).ReturnsDbSet(new List<OrderTable> { order });
        _context.Setup(x => x.orderHistory).ReturnsDbSet(history);
        _context.Setup(x => x.userTable).ReturnsDbSet(new List<UserTable> { owner });
        _context.Setup(x => x.orderHistory.Add(It.IsAny<OrderStatusHistoryTable>()))
            .Callback<OrderStatusHistoryTable>(x => history.Add(x));

        // act
        await _sut.CourierOnPlace(order.Id);

        // assert
        order.status.Should().Be(OrderStatus.CourierOnPlace);
        history.First().status.Should().Be(OrderStatus.CourierOnPlace);
        history.First().order_id.Should().Be(order.Id);
    }

    [Fact]
    public async Task OrderDelivered_WithStatusCourierOnPlace_SetOrderStatusToDelivered()
    {
        // arrange
        var owner = Generator.GenerateUser();
        var order = Generator.GenerateOrder(owner.Id, Guid.NewGuid(), OrderStatus.CourierOnPlace, Guid.NewGuid());

        var history = new List<OrderStatusHistoryTable>();

        _context.Setup(x => x.orderTable).ReturnsDbSet(new List<OrderTable> { order });
        _context.Setup(x => x.orderHistory).ReturnsDbSet(history);
        _context.Setup(x => x.userTable).ReturnsDbSet(new List<UserTable> { owner });
        _context.Setup(x => x.orderHistory.Add(It.IsAny<OrderStatusHistoryTable>()))
            .Callback<OrderStatusHistoryTable>(x => history.Add(x));

        // act
        await _sut.OrderDelivered(order.Id);

        // assert
        order.status.Should().Be(OrderStatus.Delivered);
        history.First().status.Should().Be(OrderStatus.Delivered);
        history.First().order_id.Should().Be(order.Id);
    }

    [Fact]
    public async Task OrderDelivered_WithOrderHasNoCourier_ThrowsException()
    {
        // arrange
        var owner = Generator.GenerateUser();
        var order = Generator.GenerateOrder(owner.Id, Guid.NewGuid(), OrderStatus.CourierOnPlace);

        var history = new List<OrderStatusHistoryTable>();

        _context.Setup(x => x.orderTable).ReturnsDbSet(new List<OrderTable> { order });
        _context.Setup(x => x.orderHistory).ReturnsDbSet(history);
        _context.Setup(x => x.userTable).ReturnsDbSet(new List<UserTable> { owner });
        _context.Setup(x => x.orderHistory.Add(It.IsAny<OrderStatusHistoryTable>()))
            .Callback<OrderStatusHistoryTable>(x => history.Add(x));

        // act
        Func<Task> act = async() => await _sut.OrderDelivered(order.Id);

        // assert
        await act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task CreateAsync_WithCorrectData_CreatesCourier()
    {
        // arrange
        var user = Generator.GenerateUser();

        var users = new List<UserTable> { user };

        var couriers = new List<CourierTable>();

        _context.Setup(x => x.userTable).ReturnsDbSet(users);
        _context.Setup(x => x.courierTable).ReturnsDbSet(couriers);
        _context.Setup(x => x.Add(It.IsAny<CourierTable>()))
            .Callback<CourierTable>(x => couriers.Add(x));

        var dto = new CourierDtoForCreate(user.Id, "B625CE73");

        // act
        await _sut.CreateAsync(dto);

        // assert
        couriers.First().car_number.Should().Be(dto.car_number);
        couriers.First().userId.Should().Be(dto.userId);
        couriers.Count.Should().Be(1);
    }

    [Fact]
    public async Task CreateAsync_WithCorrectDataWithoutCarNumber_CreatesCourier()
    {
        // arrange
        var user = Generator.GenerateUser();

        var users = new List<UserTable> { user };

        var couriers = new List<CourierTable>();

        _context.Setup(x => x.userTable).ReturnsDbSet(users);
        _context.Setup(x => x.courierTable).ReturnsDbSet(couriers);
        _context.Setup(x => x.Add(It.IsAny<CourierTable>()))
            .Callback<CourierTable>(x => couriers.Add(x));

        var dto = new CourierDtoForCreate(user.Id, null);

        // act
        await _sut.CreateAsync(dto);

        // assert
        couriers.First().car_number.Should().Be(null);
        couriers.First().userId.Should().Be(dto.userId);
        couriers.Count.Should().Be(1);
    }

    [Fact]
    public async Task CreateAsync_WithWrongUserId_ThrowsException()
    {
        // arrange
        var users = new List<UserTable>();

        _context.Setup(x => x.userTable).ReturnsDbSet(users);

        var dto = new CourierDtoForCreate(Guid.NewGuid(), null);

        // act
        Func<Task> act = async() => await _sut.CreateAsync(dto);

        // assert
        await act.Should().ThrowAsync<Exception>("Пользователь не найден.");
    }

    [Fact]
    public async Task UpdateAsync_WithCorrectData_UpdatesCourier()
    {
        // arrange
        var user = Generator.GenerateUser();
        var courier = Generator.GenerateCourier(user.Id, CourierStatus.IsInactive);

        _context.Setup(x => x.userTable).ReturnsDbSet(new List<UserTable> { user });
        _context.Setup(x => x.courierTable).ReturnsDbSet(new List<CourierTable> { courier });

        var dto = new CourierDtoForUpdate(courier.Id, "B358AF73", CourierStatus.IsActive);

        // act
        await _sut.UpdateAsync(dto);

        // assert
        courier.car_number.Should().Be(dto.car_number);
        courier.status.Should().Be(CourierStatus.IsActive);
    }

    [Fact]
    public async Task UpdateAsync_WithCarnumberIsNullAndTheSameStatus_UpdatesOnlyCarnumber()
    {
        // arrange
        var user = Generator.GenerateUser();
        var courier = Generator.GenerateCourier(user.Id, CourierStatus.IsActive);

        _context.Setup(x => x.userTable).ReturnsDbSet(new List<UserTable> { user });
        _context.Setup(x => x.courierTable).ReturnsDbSet(new List<CourierTable> { courier });

        var dto = new CourierDtoForUpdate(courier.Id, null, CourierStatus.IsActive);

        // act
        await _sut.UpdateAsync(dto);

        // assert
        courier.car_number.Should().BeNull();
        courier.status.Should().Be(CourierStatus.IsActive);
    }

    [Fact]
    public async Task UpdateAsync_WithWrongCourierId_ThrowsException()
    {
        // arrange
        _context.Setup(x => x.courierTable).ReturnsDbSet(new List<CourierTable>());

        var dto = new CourierDtoForUpdate(Guid.NewGuid(), null, CourierStatus.IsActive);

        // act
        Func<Task> act = async() => await _sut.UpdateAsync(dto);

        // assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Курьер не найден.");
    }

    [Fact]
    public async Task DeleteAsync_WithCorrectCourierId_DeletesCourier()
    {
        // arrange
        var courier = Generator.GenerateCourier(Guid.NewGuid());
        var couriers = new List<CourierTable> { courier };

        _context.Setup(x => x.courierTable).ReturnsDbSet(couriers);
        _context.Setup(x => x.Remove(It.IsAny<CourierTable>()))
            .Callback<CourierTable>(x => couriers.Remove(x));

        // act
        await _sut.DeleteAsync(courier.Id);

        // assert
        couriers.Count.Should().Be(0);
    }

    [Fact]
    public async Task DeleteAsync_WithWrongCourierId_ThrowsException()
    {
        // arrange
        _context.Setup(x => x.courierTable).ReturnsDbSet(new List<CourierTable>());

        // act
        Func<Task> act = async () => await _sut.DeleteAsync(Guid.NewGuid());

        // assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Курьер не найден.");
    }
}