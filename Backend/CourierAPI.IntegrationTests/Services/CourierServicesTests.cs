using CourierAPI.Service;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Middleware_Components.Broker;
using Moq;
using ORM_Components;
using ORM_Components.DTO.CourierAPI;
using ORM_Components.Interfaces;
using ORM_Components.Services;
using ORM_Components.Tables;
using ORM_Components.Tables.Helpers;
using ORM_Components.Validators.CourierValidators;
using System.Security.Claims;
using Telegram_Components.Interfaces;
using TestsBaseLib.Base;
using RabbitMQListenerServiceClient = ClientAPI.Services.RabbitMQListenerService;

namespace CourierAPI.IntegrationTests.Services;

public class CourierServicesTests : IntegrationTest
{
    private readonly CourierService _sut;
    private readonly DataContext _context;
    private readonly IMessageSender _sender;
    private readonly IRabbitMQService _rabbit;
    private readonly IMailSender _mail;
    private readonly IHttpContextAccessor _http;

    public CourierServicesTests()
    {
        _context = GetDbContext();
        _sender = Mock.Of<IMessageSender>();
        _rabbit = GetRabbitService();


        _mail = GetMailSender();
        _http = Mock.Of<IHttpContextAccessor>();

        _sut = new CourierService(_context, _sender, _rabbit, _mail, 
            new CourierValidatorDtoForCreate(), new CourierValidatorDtoForUpdate(), _http);

        ClearDatabase(_context);
    }
    private void setUser(Guid id)
    {
        Mock.Get(_http).Setup(x => x.HttpContext.User.FindFirst("Id")).Returns(new Claim("UserId", id.ToString()));
    }

    [Fact]
    public async Task OrderDelivered_WithCorrectData_UpdatesOrderStatusAndSendRabbitMessages()
    {
        // arrange
        var courierUser = Generator.GenerateUser();
        var client = Generator.GenerateUser();
        client.email = Configuration["EMAILRECEIVER"];
        var owner = Generator.GenerateUser();

        var courier = Generator.GenerateCourier(courierUser.Id, CourierStatus.IsActive);
        var rest = Generator.GenerateRestaurant(owner.Id, RestaurantStatus.Verified);
        var order = Generator.GenerateOrder(client.Id, rest.Id, OrderStatus.CourierOnPlace, courier.Id);

        var item1 = Generator.GenerateFoodItem(rest.Id, 249);
        var item2 = Generator.GenerateFoodItem(rest.Id, 549);
        var item3 = Generator.GenerateFoodItem(rest.Id, 199);
        order.total_price = item1.price + item2.price + item3.price;

        var orderItem1 = new OrderItemsTable { order_id = order.Id, restaraunt_food_item = item1.Id };
        var orderItem2 = new OrderItemsTable { order_id = order.Id, restaraunt_food_item = item2.Id };
        var orderItem3 = new OrderItemsTable { order_id = order.Id, restaraunt_food_item = item3.Id };
        var orderItem4 = new OrderItemsTable { order_id = order.Id, restaraunt_food_item = item1.Id };

        var listener = new RabbitMQListenerService(_mail, _rabbit);
        var clientListener = new RabbitMQListenerServiceClient(GetDataService(_context),
            _sender, _rabbit);

        _context.userTable.AddRange(courierUser, client, owner);
        _context.courierTable.Add(courier);
        _context.restaurantTable.Add(rest);
        _context.orderTable.Add(order);
        _context.restaurantFoodItemsTable.AddRange(item1, item2, item3);
        _context.orderItemsTable.AddRange(orderItem1, orderItem2, orderItem3);
        _context.SaveChanges();

        await listener.StartAsync(new CancellationToken());
        await clientListener.StartAsync(new CancellationToken());

        // act
        await _sut.OrderDelivered(order.Id);
        await Task.Delay(1000);

        // assert

        await listener.StopAsync(new CancellationToken());
        await clientListener.StopAsync(new CancellationToken());

        var expectedOrder = _context.orderTable.First();
        expectedOrder.status.Should().Be(OrderStatus.Delivered);

        var history = _context.orderHistory.First();
        history.status.Should().Be(OrderStatus.Delivered);
        history.order_id.Should().Be(order.Id);

        _context.reviewTable.Count().Should().Be(1);
    }
}
