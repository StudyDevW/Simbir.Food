using CourierAPI.Service;
using FluentAssertions;
using Moq;
using Moq.EntityFrameworkCore;
using ORM_Components;
using ORM_Components.DTO.CourierAPI;
using ORM_Components.Tables;
using ORM_Components.Tables.Helpers;
using Telegram_Components.Interfaces;
using TestsBaseLib.Base;

namespace CourierAPI.Tests.Services;

public class CourierServiceTests
{
    [Fact]
    public async Task AcceptOrder_WithCorrectData_SetCourierOfAnOrder()
    {
        // arrange
        var context = new Mock<DataContext>();
        var sender = new Mock<IMessageSender>();

        var order = Generator.GenerateOrder(OrderStatus.Ready);

        var courier = new CourierTable
        {
            Id = Guid.NewGuid(),
        };

        context.Setup(x => x.orderTable).ReturnsDbSet(new List<OrderTable> { order });

        context.Setup(x => x.courierTable).ReturnsDbSet(new List<CourierTable> { courier });

        var sut = new CourierService(context.Object, sender.Object);
        var dto = new OrderLinkCourierDto(order.Id, courier.Id);

        // act
        await sut.AcceptOrder(dto);

        // asset
        order.courier_id.Should().Be(courier.Id);
    }

    [Fact]
    public async Task AcceptOrder_WithWrongOrderId_ThrowsOrderException()
    {
        // arrange
        var context = new Mock<DataContext>();
        var sender = new Mock<IMessageSender>();

        var order = Generator.GenerateOrder(OrderStatus.Ready);

        var courier = new CourierTable
        {
            Id = Guid.NewGuid(),
        };

        context.Setup(x => x.orderTable).ReturnsDbSet(new List<OrderTable> { order });

        context.Setup(x => x.courierTable).ReturnsDbSet(new List<CourierTable> { courier });

        var sut = new CourierService(context.Object, sender.Object);
        var dto = new OrderLinkCourierDto(Guid.NewGuid(), courier.Id);

        // act
        Func<Task> act = async () => await sut.AcceptOrder(dto);

        // assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Заказ не найден.");
    }

    [Fact]
    public async Task AcceptOrder_WithWrongCourierId_ThrowsCourierException()
    {
        // arrange
        var context = new Mock<DataContext>();
        var sender = new Mock<IMessageSender>();

        var order = Generator.GenerateOrder(OrderStatus.Ready);

        var courier = new CourierTable
        {
            Id = Guid.NewGuid(),
        };

        context.Setup(x => x.orderTable).ReturnsDbSet(new List<OrderTable> { order });

        context.Setup(x => x.courierTable).ReturnsDbSet(new List<CourierTable> { courier });

        var sut = new CourierService(context.Object, sender.Object);
        var dto = new OrderLinkCourierDto(order.Id, Guid.NewGuid());

        // act
        Func<Task> act = async () => await sut.AcceptOrder(dto);

        // assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Курьер не найден.");
    }

    [Fact]
    public async Task TakeOrder_WithStatusReady_SetOrderStatusToWaitingForDelivery()
    {
        // arrange
        var context = new Mock<DataContext>();
        var sender = new Mock<IMessageSender>();

        var owner = Generator.GenerateUser("log1", "pas1", new string[] { "Client" });
        var order = Generator.GenerateOrder(owner.Id, Guid.NewGuid(), OrderStatus.Ready);

        var history = new List<OrderStatusHistoryTable>();

        context.Setup(x => x.orderTable).ReturnsDbSet(new List<OrderTable> { order });
        context.Setup(x => x.orderStatusHistoryTables).ReturnsDbSet(history);
        context.Setup(x => x.userTable).ReturnsDbSet(new List<UserTable> { owner });
        context.Setup(x => x.orderStatusHistoryTables.Add(It.IsAny<OrderStatusHistoryTable>()))
            .Callback<OrderStatusHistoryTable>(x => history.Add(x));

        var sut = new CourierService(context.Object, sender.Object);

        // act
        await sut.TakeOrder(order.Id);

        // assert
        order.status.Should().Be(OrderStatus.WaitingForDelivery);
        history.First().status.Should().Be(OrderStatus.WaitingForDelivery);
        history.First().order_id.Should().Be(order.Id);
    }

    [Fact]
    public async Task TakeOrder_WithNotExpectedStatus_ThrowsException()
    {
        // arrange
        var context = new Mock<DataContext>();
        var sender = new Mock<IMessageSender>();

        var order = Generator.GenerateOrder(OrderStatus.WaitingForDelivery);

        context.Setup(x => x.orderTable).ReturnsDbSet(new List<OrderTable> { order });

        var sut = new CourierService(context.Object, sender.Object);

        // act
        Func<Task> act = async() => await sut.TakeOrder(order.Id);

        // assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Статус не соответствует ожидаемому.");
    }

    [Fact]
    public async Task TakeOrder_WithWrongOrderId_ThrowsException()
    {
        // arrange
        var context = new Mock<DataContext>();
        var sender = new Mock<IMessageSender>();

        var order = Generator.GenerateOrder(OrderStatus.Ready);

        context.Setup(x => x.orderTable).ReturnsDbSet(new List<OrderTable> { order });

        var sut = new CourierService(context.Object, sender.Object);

        // act
        Func<Task> act = async () => await sut.TakeOrder(Guid.NewGuid());

        // assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Заказ не найден.");
    }

    [Fact]
    public async Task CourierOnPlace_WithStatusWaitingForDelivery_SetOrderStatusToCourierOnPlace()
    {
        // arrange
        var context = new Mock<DataContext>();
        var sender = new Mock<IMessageSender>();

        var owner = Generator.GenerateUser("log1", "pas1", new string[] { "Client" });
        var order = Generator.GenerateOrder(owner.Id, Guid.NewGuid(), OrderStatus.WaitingForDelivery);

        var history = new List<OrderStatusHistoryTable>();

        context.Setup(x => x.orderTable).ReturnsDbSet(new List<OrderTable> { order });
        context.Setup(x => x.orderStatusHistoryTables).ReturnsDbSet(history);
        context.Setup(x => x.userTable).ReturnsDbSet(new List<UserTable> { owner });
        context.Setup(x => x.orderStatusHistoryTables.Add(It.IsAny<OrderStatusHistoryTable>()))
            .Callback<OrderStatusHistoryTable>(x => history.Add(x));

        var sut = new CourierService(context.Object, sender.Object);

        // act
        await sut.CourierOnPlace(order.Id);

        // assert
        order.status.Should().Be(OrderStatus.CourierOnPlace);
        history.First().status.Should().Be(OrderStatus.CourierOnPlace);
        history.First().order_id.Should().Be(order.Id);
    }

    [Fact]
    public async Task OrderDelivered_WithStatusCourierOnPlace_SetOrderStatusToDelivered()
    {
        // arrange
        var context = new Mock<DataContext>();
        var sender = new Mock<IMessageSender>();

        var owner = Generator.GenerateUser("log1", "pas1", new string[] { "Client" });
        var order = Generator.GenerateOrder(owner.Id, Guid.NewGuid(), OrderStatus.CourierOnPlace);

        var history = new List<OrderStatusHistoryTable>();

        context.Setup(x => x.orderTable).ReturnsDbSet(new List<OrderTable> { order });
        context.Setup(x => x.orderStatusHistoryTables).ReturnsDbSet(history);
        context.Setup(x => x.userTable).ReturnsDbSet(new List<UserTable> { owner });
        context.Setup(x => x.orderStatusHistoryTables.Add(It.IsAny<OrderStatusHistoryTable>()))
            .Callback<OrderStatusHistoryTable>(x => history.Add(x));

        var sut = new CourierService(context.Object, sender.Object);

        // act
        await sut.OrderDelivered(order.Id);

        // assert
        order.status.Should().Be(OrderStatus.Delivered);
        history.First().status.Should().Be(OrderStatus.Delivered);
        history.First().order_id.Should().Be(order.Id);
    }

    [Fact]
    public async Task CreateAsync_WithCorrectData_CreatesCourier()
    {
        // arrange
        var context = new Mock<DataContext>();
        var sender = new Mock<IMessageSender>();

        var user = Generator.GenerateUser();

        var users = new List<UserTable> { user };

        var couriers = new List<CourierTable>();

        context.Setup(x => x.userTable).ReturnsDbSet(users);
        context.Setup(x => x.courierTable).ReturnsDbSet(couriers);
        context.Setup(x => x.Add(It.IsAny<CourierTable>()))
            .Callback<CourierTable>(x => couriers.Add(x));

        var dto = new CourierDtoForCreate(user.Id, "B625CE73");

        var sut = new CourierService(context.Object, sender.Object);

        // act
        await sut.CreateAsync(dto);

        // assert
        couriers.First().car_number.Should().Be(dto.car_number);
        couriers.First().userId.Should().Be(dto.userId);
        couriers.Count.Should().Be(1);
    }

    [Fact]
    public async Task CreateAsync_WithCorrectDataWithoutCarNumber_CreatesCourier()
    {
        // arrange
        var context = new Mock<DataContext>();
        var sender = new Mock<IMessageSender>();

        var user = Generator.GenerateUser();

        var users = new List<UserTable> { user };

        var couriers = new List<CourierTable>();

        context.Setup(x => x.userTable).ReturnsDbSet(users);
        context.Setup(x => x.courierTable).ReturnsDbSet(couriers);
        context.Setup(x => x.Add(It.IsAny<CourierTable>()))
            .Callback<CourierTable>(x => couriers.Add(x));

        var dto = new CourierDtoForCreate(user.Id, null);

        var sut = new CourierService(context.Object, sender.Object);

        // act
        await sut.CreateAsync(dto);

        // assert
        couriers.First().car_number.Should().Be(null);
        couriers.First().userId.Should().Be(dto.userId);
        couriers.Count.Should().Be(1);
    }

    [Fact]
    public async Task CreateAsync_WithWrongUserId_ThrowsException()
    {
        // arrange
        var context = new Mock<DataContext>();
        var sender = new Mock<IMessageSender>();

        var users = new List<UserTable>();

        context.Setup(x => x.userTable).ReturnsDbSet(users);

        var dto = new CourierDtoForCreate(Guid.NewGuid(), null);

        var sut = new CourierService(context.Object, sender.Object);

        // act
        Func<Task> act = async() => await sut.CreateAsync(dto);

        // assert
        await act.Should().ThrowAsync<Exception>("Пользователь не найден.");
    }
}