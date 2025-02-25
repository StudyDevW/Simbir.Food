using FluentAssertions;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using Middleware_Components.JWT.DTO.Token;
using Middleware_Components.Services;
using Moq;
using Moq.EntityFrameworkCore;
using ORM_Components;
using ORM_Components.DTO.RestaurantAPI;
using ORM_Components.Tables;
using ORM_Components.Tables.Helpers;
using RestaurantAPI.Model.Controllers;
using TestsBaseLib.Base;

namespace RestaurantAPI.Tests.Controllers;

public class OrderControllerTests
{
    private readonly OrderController _sut;
    private readonly Mock<DataContext> _context;
    private readonly Mock<IJwtService> _jwt;

    private readonly List<OrderTable> _orders;

    public OrderControllerTests()
    {
        _jwt = new Mock<IJwtService>();
        _context = new Mock<DataContext>();
        _orders = new List<OrderTable>();

        _context.Setup(x => x.orderTable).ReturnsDbSet(_orders);
        _context.Setup(x => x.orderTable.Add(It.IsAny<OrderTable>()))
            .Callback<OrderTable>(x => _orders.Add(x));
        _context.Setup(x => x.orderTable.Remove(It.IsAny<OrderTable>()))
            .Callback<OrderTable>(x => _orders.Remove(x));
        _context.Setup(x => x.orderTable.FindAsync(It.IsAny<object>()))
            .Returns<object[]>((x) =>
            {
                var guid = (Guid)x.First();
                var order = _orders.FirstOrDefault(c => c.Id == guid);
                var task = ValueTask.FromResult(order);
                return task;
            });

        _sut = new OrderController(_context.Object, _jwt.Object);
        _sut.ControllerContext = new ControllerContext();
        _sut.ControllerContext.HttpContext = new DefaultHttpContext();
        _sut.ControllerContext.HttpContext.Request.Headers.Authorization = "auth";
    }

    private Order_DTO mapOrderTableToOrderDto(OrderTable table)
    {
        return new Order_DTO
        {
            restaurant_id = table.restaurant_id,
            client_id = table.client_id,
            courier_id = table.courier_id,
            id = table.Id,
            order_date = table.order_date,
            status = table.status,
            total_price = table.total_price
        };
    }

    [Fact]
    public async Task CreateOrder_WithCorrectOrderDto_ReturnsSuccessResult()
    {
        // arrange
        var order = Generator.GenerateOrder(OrderStatus.Accepted);
        var dto = mapOrderTableToOrderDto(order);

        //act 
        var result = await _sut.CreateOrder(dto);

        // assert
        result.Should().BeOfType<OkObjectResult>();
        _orders.Count.Should().Be(1);
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000", 
        "43eceb31-2caf-4edf-95c8-d3eefe4d8239",
        "cf5aafe2-2014-4ab8-80e8-78f5b954d952", 200)]
    [InlineData("43eceb31-2caf-4edf-95c8-d3eefe4d8239",
        "00000000-0000-0000-0000-000000000000",
        "cf5aafe2-2014-4ab8-80e8-78f5b954d952", 200)]
    [InlineData("43eceb31-2caf-4edf-95c8-d3eefe4d8239",
        "43eceb31-2caf-4edf-95c8-d3eefe4d8239",
        "00000000-0000-0000-0000-000000000000", 200)]
    [InlineData("43eceb31-2caf-4edf-95c8-d3eefe4d8239",
        "43eceb31-2caf-4edf-95c8-d3eefe4d8239",
        "43eceb31-2caf-4edf-95c8-d3eefe4d8239", -500)]
    public async Task CreateOrder_WithWrongData_ReturnsBadRequest(Guid restaurant_id, Guid client_id, Guid courier_id, int total_price)
    {
        // arrange
        var dto = new Order_DTO
        {
            restaurant_id = restaurant_id,
            courier_id = courier_id,
            total_price = total_price,
            status = OrderStatus.Accepted,
            client_id = client_id,
            order_date = DateTime.UtcNow,
        };

        //act 
        var result = await _sut.CreateOrder(dto);

        // assert
        result.Should().BeOfType<BadRequestObjectResult>();
        _orders.Count.Should().Be(0);
    }

    [Fact]
    public async Task GetOrderById_WithCorrectOrderId_ReturnsOrderTable()
    {
        // arrange
        var order = Generator.GenerateOrder(OrderStatus.Accepted);
        _orders.Add(order);

        //act 
        var result = await _sut.GetOrderById(order.Id);

        // assert
        result.Should().BeOfType<OkObjectResult>();

        var obj = result as OkObjectResult;
        obj.Value.Should().BeOfType<OrderTable>();

        var okOrder = obj.Value as OrderTable;
        okOrder.Should().BeEquivalentTo(order);
    }

    [Fact]
    public async Task GetOrderById_WithWrongOrderId_ReturnsNotFound()
    {
        // arrange
        var id = Guid.NewGuid();

        //act 
        var result = await _sut.GetOrderById(id);

        // assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task UpdateOrder_WithCorrectOrderDto_ReturnsSuccessResult()
    {
        // arrange
        var order = Generator.GenerateOrder(OrderStatus.Accepted);
        _orders.Add(order);

        var dto = new Order_DTO
        {
            status = OrderStatus.WaitingForDelivery,
            total_price = 500
        };

        //act 
        var result = await _sut.UpdateOrder(order.Id, dto);

        // assert
        result.Should().BeOfType<OkObjectResult>();
        order.status.Should().Be(dto.status);
        order.total_price.Should().Be(dto.total_price);
    }

    [Fact]
    public async Task UpdateOrder_WithPriceEqualsZero_ReturnsBadRequest()
    {
        // arrange
        var order = Generator.GenerateOrder(OrderStatus.Accepted);
        _orders.Add(order);

        var dto = new Order_DTO
        {
            status = OrderStatus.WaitingForDelivery,
            total_price = 0
        };

        //act 
        var result = await _sut.UpdateOrder(order.Id, dto);

        // assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UpdateOrder_WithWrongOrderId_ReturnsNotFound()
    {
        // arrange
        var dto = new Order_DTO
        {
            status = OrderStatus.WaitingForDelivery,
            total_price = 50
        };

        // act 
        var result = await _sut.UpdateOrder(Guid.NewGuid(), dto);

        // assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task DeleteOrder_WithCorrectOrderId_ReturnsSuccessResult()
    {
        // arrange
        var order = Generator.GenerateOrder(OrderStatus.Accepted);
        _orders.Add(order);

        // act
        var result = await _sut.DeleteOrder(order.Id);

        // assert
        result.Should().BeOfType<OkObjectResult>();
        _orders.Count.Should().Be(0);
    }

    [Fact]
    public async Task DeleteOrder_WithWrongOrderId_ReturnsNotFound()
    {
        // arrange
        var orderId = Guid.NewGuid();

        // act
        var result = await _sut.DeleteOrder(orderId);

        // assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    private void initRestaurants(List<RestaurantTable> restaurants)
    {
        _context.Setup(x => x.restaurantTable).ReturnsDbSet(restaurants);

        _context.Setup(x => x.restaurantTable.FindAsync(It.IsAny<object>()))
            .Returns<object[]>((x) =>
            {
                var guid = (Guid)x.First();
                var restaurant = restaurants.FirstOrDefault(c => c.Id == guid);
                var task = ValueTask.FromResult(restaurant);
                return task;
            });

        _jwt.Setup(x => x.AccessTokenValidation(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new Token_ValidProperties
            {
                token_success = new Token_ValidSuccess
                {
                    Id = Guid.NewGuid(),
                    telegramChatId = "32859325"
                }
            });
    }

    [Fact]
    public async Task OrderFromRestaurant_WithCorrectData_ReturnsSuccessResult_1()
    {
        // arrange
        var rest = new RestaurantTable
        {
            Id = Guid.NewGuid(),
            open_time = DateTime.UtcNow.AddMinutes(-10),
            close_time = DateTime.UtcNow.AddMinutes(10)
        };

        var rests = new List<RestaurantTable> { rest };
        initRestaurants(rests);

        // act
        var result = await _sut.OrderFromRestaurant(rest.Id);

        // assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task OrderFromRestaurant_WithCorrectData_ReturnsSuccessResult_2()
    {
        // arrange
        var rest = new RestaurantTable
        {
            Id = Guid.NewGuid(),

            // Тут проблема в датах, мы должны сравнивать только время, для этого этот тест существует
            open_time = DateTime.UtcNow.AddDays(-1).AddHours(-1),
            close_time = DateTime.UtcNow.AddDays(-1).AddHours(+1)
        };

        var rests = new List<RestaurantTable> { rest };
        initRestaurants(rests);

        // act
        var result = await _sut.OrderFromRestaurant(rest.Id);

        // assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task OrderFromRestaurant_WithWrongRestaurantId_ReturnsNotFound()
    {
        // arrange
        var rests = new List<RestaurantTable>();
        initRestaurants(rests);

        // act
        var result = await _sut.OrderFromRestaurant(Guid.NewGuid());

        // assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task OrderFromRestaurant_WithClosedRestaurant_ReturnsBadRequest_1()
    {
        // arrange
        var rest = new RestaurantTable
        {
            Id = Guid.NewGuid(),
            open_time = DateTime.UtcNow.AddMinutes(10),
            close_time = DateTime.UtcNow.AddMinutes(-10)
        };

        var rests = new List<RestaurantTable> { rest };
        initRestaurants(rests);

        // act
        var result = await _sut.OrderFromRestaurant(rest.Id);

        // assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }
}
