using CourierAPI.Service;
using FluentAssertions;
using Moq;
using Moq.EntityFrameworkCore;
using ORM_Components;
using ORM_Components.DTO.CourierAPI;
using ORM_Components.Tables;
using ORM_Components.Tables.Helpers;
using StackExchange.Redis;
using TestsBaseLib.Base;

namespace CourierAPI.Tests.Services;

public class CourierServiceTests
{
    [Fact]
    public async Task AcceptOrder_WithCorrectData_SetCourierOfAnOrder()
    {
        // arrange
        var context = new Mock<DataContext>();

        var order = new OrderTable
        {
            Id = Guid.NewGuid(),
            courier_id = null,
        };

        var courier = new CourierTable
        {
            Id = Guid.NewGuid(),
        };

        context.Setup(x => x.orderTable).ReturnsDbSet(new List<OrderTable> { order });

        context.Setup(x => x.courierTable).ReturnsDbSet(new List<CourierTable> { courier });

        var sut = new CourierService(context.Object);
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

        var order = new OrderTable
        {
            Id = Guid.NewGuid(),
            courier_id = null,
        };

        var courier = new CourierTable
        {
            Id = Guid.NewGuid(),
        };

        context.Setup(x => x.orderTable).ReturnsDbSet(new List<OrderTable> { order });

        context.Setup(x => x.courierTable).ReturnsDbSet(new List<CourierTable> { courier });

        var sut = new CourierService(context.Object);
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

        var order = new OrderTable
        {
            Id = Guid.NewGuid(),
            courier_id = null,
        };

        var courier = new CourierTable
        {
            Id = Guid.NewGuid(),
        };

        context.Setup(x => x.orderTable).ReturnsDbSet(new List<OrderTable> { order });

        context.Setup(x => x.courierTable).ReturnsDbSet(new List<CourierTable> { courier });

        var sut = new CourierService(context.Object);
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

        var order = new OrderTable
        {
            Id = Guid.NewGuid(),
            status = OrderStatus.Ready
        };

        context.Setup(x => x.orderTable).ReturnsDbSet(new List<OrderTable> { order });

        var sut = new CourierService(context.Object);

        // act
        await sut.TakeOrder(order.Id);

        // assert
        order.status.Should().Be(OrderStatus.WaitingForDelivery);
    }

    [Fact]
    public async Task TakeOrder_WithNotExpectedStatus_ThrowsException()
    {
        // arrange
        var context = new Mock<DataContext>();

        var order = new OrderTable
        {
            Id = Guid.NewGuid(),
            status = OrderStatus.WaitingForDelivery
        };

        context.Setup(x => x.orderTable).ReturnsDbSet(new List<OrderTable> { order });

        var sut = new CourierService(context.Object);

        // act
        Func<Task> act = async() => await sut.TakeOrder(order.Id);

        // assert
        await act.Should().ThrowAsync<Exception>().WithMessage("Статус не соответствует требуемому.");
    }

    [Fact]
    public async Task TakeOrder_WithWrongOrderId_ThrowsException()
    {
        // arrange
        var context = new Mock<DataContext>();

        var order = new OrderTable
        {
            Id = Guid.NewGuid(),
            status = OrderStatus.Ready
        };

        context.Setup(x => x.orderTable).ReturnsDbSet(new List<OrderTable> { order });

        var sut = new CourierService(context.Object);

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

        var order = new OrderTable
        {
            Id = Guid.NewGuid(),
            status = OrderStatus.WaitingForDelivery
        };

        context.Setup(x => x.orderTable).ReturnsDbSet(new List<OrderTable> { order });

        var sut = new CourierService(context.Object);

        // act
        await sut.CourierOnPlace(order.Id);

        // assert
        order.status.Should().Be(OrderStatus.CourierOnPlace);
    }

    [Fact]
    public async Task OrderDelivered_WithStatusCourierOnPlace_SetOrderStatusToDelivered()
    {
        // arrange
        var context = new Mock<DataContext>();

        var order = new OrderTable
        {
            Id = Guid.NewGuid(),
            status = OrderStatus.CourierOnPlace
        };

        context.Setup(x => x.orderTable).ReturnsDbSet(new List<OrderTable> { order });

        var sut = new CourierService(context.Object);

        // act
        await sut.OrderDelivered(order.Id);

        // assert
        order.status.Should().Be(OrderStatus.Delivered);
    }
}