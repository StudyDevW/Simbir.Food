using ClientAPI.Interfaces;
using ClientAPI.Services;
using FluentAssertions;
using Middleware_Components.Broker;
using Middleware_Components.DTO.ClientAPI;
using Middleware_Components.Services;
using Moq;
using ORM_Components;
using ORM_Components.DTO.ClientAPI;
using ORM_Components.DTO.ClientAPI.RequestsAll;
using ORM_Components.Tables;
using ORM_Components.Tables.Helpers;
using StackExchange.Redis;
using System.Diagnostics.CodeAnalysis;
using Telegram_Components.Interfaces;
using TestsBaseLib.Base;
using RabbitMQListenerServiceRestaurant = RestaurantAPI.Services.RabbitMQListenerService;

namespace ClientAPI.IntegrationTests.Services;

public class ClientServicesTests : IntegrationTest
{
    private readonly ClientService _sut;
    private readonly RabbitMQListenerServiceRestaurant _listener;
    private readonly DataContext _context;
    private readonly ICacheService _cache;
    private readonly IDatabase _cacheDatabase;
    private readonly ISessionService _session;
    private readonly IRabbitMQService _rabbit;

    public ClientServicesTests()
    {
        var rabbit = GetRabbitService();
        _rabbit = rabbit;

        var tgsender = Mock.Of<IMessageSender>();

        var multiplexer = GetConnectionMultiplexer();
        _cacheDatabase = multiplexer.GetDatabase();

        var cache = GetCacheService(multiplexer);
        _cache = cache;

        var session = GetSessionService(cache);
        _session = session;

        var context = GetDbContext();
        _context = context;

        var database = GetDataService(context);
        var jwt = GetJwtService(cache);

        _listener = new RabbitMQListenerServiceRestaurant(rabbit, tgsender, context);

        _sut = new ClientService(rabbit, tgsender, session, database, jwt, cache);

        ClearDatabase(context);
        //ClearRedis();
    }

    /// <summary>
    /// Данный метод авторизирует нового пользователя и возвращает пользователя + Bearer токен
    /// </summary>
    private async Task<(UserTable user, string bearer, string refresh, string access)> createUser(string roles = "Client", int money = 0)
    {
        var user = Generator.GenerateUser("Admin");
        user.money_value = money;

        _context.userTable.Add(user);
        _context.SaveChanges();

        var tokens = await _sut.UserAuth(user.ToDto("PC"));
        return (user, "Bearer " + tokens!.accessToken, tokens!.refreshToken!, tokens!.accessToken!);
    }

    [Fact]
    public async Task UserRegister_WithCorrectData_CreatesNewUser()
    {
        // arrange
        var user = Generator.GenerateUser();
        var dto = user.ToDto(device: "PC");

        var reciever = GetTelegramMessageReceiver(GetBotClient(), GetOperations(_context), _cache);

        // act
        var result = await _sut.UserRegister(dto);

        // assert
        var obj = _cache.GetKeyFromStorage<AuthAddUser>($"register_request_{dto.id}");

        obj.Should().BeEquivalentTo(dto);
        result.Should().Be("register_request_created");
    }

    [Fact]
    public async Task UserAuth_WithCorrectData_ReturnsTokens()
    {
        // arrange
        var user = Generator.GenerateUser();
        
        _context.userTable.Add(user);
        _context.SaveChanges();

        var dto = user.ToDto("PC");

        // act
        var tokens = await _sut.UserAuth(dto);

        // assert
        tokens.Should().NotBeNull();
        tokens.accessToken.Should().NotBeNull();
        tokens.refreshToken.Should().NotBeNull();

        _cache.CheckExistKeysStorage(user.Id, "accessTokens").Should().BeTrue();
        _cache.CheckExistKeysStorage(user.Id, "refreshTokens").Should().BeTrue();

        var sessions = _session.GetSessions(user.Id);
        sessions!.First().statusSession.Should().Be("active");
    }

    [Fact]
    public async Task ClientSignOut_WithCorrectBearerKey_DeletesTokens()
    {
        // arrange
        var user = Generator.GenerateUser();

        _context.userTable.Add(user);
        _context.SaveChanges();

        var dto = user.ToDto("PC");
        var tokens = await _sut.UserAuth(dto);
        var bearer = "Bearer " + tokens!.accessToken!;

        // act
        var result = await _sut.ClientSignOut(bearer);

        // assert
        result.Should().Contain("is_logout");
        
        _cache.CheckExistKeysStorage(user.Id, "accessTokens").Should().BeFalse();
        _cache.CheckExistKeysStorage(user.Id, "refreshTokens").Should().BeFalse();

        var sessions = _session.GetSessions(user.Id);
        sessions!.First().statusSession.Should().Be("expired");
        sessions!.First().timeDel.HasValue.Should().BeTrue();
    }

    [Fact]
    public async Task RefreshClientSession_WithCorrectData_ReturnsNewTokens()
    {
        // arrange
        var user = await createUser();

        var dto = new Auth_RefreshTokens
        {
            refreshToken = user.refresh
        };

        // act
        var result = await _sut.RefreshClientSession(dto);

        // assert
        result.Should().NotBeNull();
        result.accessToken.Should().NotBeNull();
        result.refreshToken.Should().NotBeNull();

        user.refresh.Should().NotBe(result.refreshToken);
        user.access.Should().NotBe(result.accessToken);

        var sessions = _session.GetSessions(user.user.Id);
        sessions!.First().statusSession.Should().Be("active");
        sessions!.First().timeUpd.HasValue.Should().BeTrue();
    }

    [Fact]
    public async Task ClientFromIdInfo_WithCorrectData_ReturnsClientInfo()
    {
        // arrange
        var admin = await createUser("Admin");
        var user = await createUser();

        var rest = Generator.GenerateRestaurant(user.user.Id, RestaurantStatus.Verified);
        var food = Generator.GenerateFoodItem(rest.Id);
        var basket = Generator.GenBasket(user.user.Id, food.Id);

        _context.restaurantTable.Add(rest);
        _context.restaurantFoodItemsTable.Add(food);
        _context.basketTable.Add(basket);
        _context.SaveChanges();

        // act
        var result = await _sut.ClientFromIdInfo(admin.bearer, user.user.Id);

        // assert
        result.Should().Be(user.user, new UserClientInfoComparer());
        result.basket_items.Should().Be(1);
        result.restaurant_own!.First().Should().Be(rest.Id);
    }

    [Fact]
    public async Task ClientMeInfo_WithCorrectData_ReturnsMyClientInfo()
    {
        // arrange
        var creation = await createUser();
        var user = creation.user;
        var bearer = creation.bearer;

        var rest = Generator.GenerateRestaurant(user.Id, RestaurantStatus.Verified);
        var food = Generator.GenerateFoodItem(rest.Id);
        var basket = Generator.GenBasket(user.Id, food.Id);

        _context.restaurantTable.Add(rest);
        _context.restaurantFoodItemsTable.Add(food);
        _context.basketTable.Add(basket);
        _context.SaveChanges();

        // act
        var result = await _sut.ClientMeInfo(bearer);

        // assert
        result.Should().Be(user, new UserClientInfoComparer());
        result.basket_items.Should().Be(1);
        result.restaurant_own!.First().Should().Be(rest.Id);
    }

    [Fact]
    public async Task AllProfilesGet_WithCorrectData_ReturnsProfiles()
    {
        // arrange
        var admin = await createUser("Admin");

        var user1 = Generator.GenerateUser();
        var user2 = Generator.GenerateUser();
        var user3 = Generator.GenerateUser();
        var user4 = Generator.GenerateUser();

        _context.userTable.AddRange(user1, user2, user3, user4);
        _context.SaveChanges();

        // act
        var result = await _sut.AllProfilesGet(admin.bearer, 3, 2);

        // assert
        result.Should().NotBeNull();
        result.Content.Count.Should().Be(2);
    }

    [Fact]
    public async Task DeleteClientWithAdmin_WithCorrectData_DeletesClient()
    {
        // arrange
        var admin = await createUser("Admin");
        var user = await createUser();

        var rest = Generator.GenerateRestaurant(user.user.Id, RestaurantStatus.Verified);
        var courier = Generator.GenerateCourier(user.user.Id, CourierStatus.IsInactive);

        var food = Generator.GenerateFoodItem(rest.Id);

        _context.restaurantTable.Add(rest);
        _context.courierTable.Add(courier);
        _context.restaurantFoodItemsTable.Add(food);
        _context.SaveChanges();

        // act
        await _sut.DeleteClientWithAdmin(admin.bearer, user.user.Id);

        // assert
        _context.userTable.Count().Should().Be(1);
        _context.userTable.First().Id.Should().Be(admin.user.Id);

        _context.restaurantTable.Count().Should().Be(0);
        _context.courierTable.Count().Should().Be(0);
        _context.restaurantFoodItemsTable.Count().Should().Be(0);
    }

    [Fact]
    public async Task AddBasketItem_WithCorrectData_AddsBasketItem()
    {
        // arrange
        var user = await createUser();

        var owner = await createUser();
        var rest = Generator.GenerateRestaurant(owner.user.Id, RestaurantStatus.Verified);
        var food = Generator.GenerateFoodItem(rest.Id);

        _context.restaurantTable.Add(rest);
        _context.restaurantFoodItemsTable.Add(food);
        _context.SaveChanges();

        var dto = new Basket_Add
        {
            user_id = user.user.Id,
            food_item_id = food.Id
        };

        // act
        await _sut.AddBasketItem(user.bearer, dto);

        // assert
        _context.basketTable.Count().Should().Be(1);
        _context.basketTable.First().user_id.Should().Be(user.user.Id);
        _context.basketTable.First().food_item_id.Should().Be(food.Id);
    }

    [Fact]
    public async Task GetItemsBasket_WithCorrectData_ReturnsBasketItems()
    {
        // arrange
        var user = await createUser();

        var rest = Generator.GenerateRestaurant(user.user.Id, RestaurantStatus.Verified);
        var food1 = Generator.GenerateFoodItem(rest.Id);
        var basket1 = Generator.GenBasket(user.user.Id, food1.Id);
        var basket2 = Generator.GenBasket(user.user.Id, food1.Id);

        _context.restaurantTable.Add(rest);
        _context.restaurantFoodItemsTable.Add(food1);
        _context.basketTable.AddRange(basket1, basket2);
        _context.SaveChanges();

        // act
        var result = await _sut.GetItemsBasket(user.bearer);

        // assert
        result!.basketInfo!.totalPrice.Should().Be(food1.price * 2);
        result!.basketItem!.Count.Should().Be(2);
    }

    [Fact]
    public async Task GetAllRequestsForAdmin_WithCorrectData_ReturnsRequests()
    {
        // arrange
        var admin = await createUser("Admin");
        var user = await createUser();

        var rest = Generator.GenerateRestaurant(user.user.Id, RestaurantStatus.Unverified);
        var courier = Generator.GenerateCourier(user.user.Id, CourierStatus.Unverified);

        var req1 = Generator.GenRestaurantRequest(user.user.Id, rest.Id);
        var req2 = Generator.GenCourierRequest(user.user.Id, courier.Id);

        _context.restaurantTable.Add(rest);
        _context.courierTable.Add(courier);
        _context.requestTable.AddRange(req1, req2);
        _context.SaveChanges();

        // act
        var result = await _sut.GetAllRequestsForAdmin(admin.bearer);

        // assert
        result!.courier_requests.Count.Should().Be(1);
        result.restaurant_requests.Count.Should().Be(1);

        result.restaurant_requests.First().Should().Be((rest, req1, user.user), new RequestInfoRestaurantComparer());
        result.courier_requests.First().Should().Be((courier, req2, user.user), new RequestInfoCourierComparer());
    }

    [Fact]
    public async Task CreateRestaurantRequest_WithCorrectData_CreatesRestaurantRequest()
    {
        // arrange
        var user = await createUser();

        var rest = Generator.GenerateRestaurant(user.user.Id, RestaurantStatus.Unverified);
        var dto = rest.ToRequestDto("please");

        // act
        await _sut.CreateRestaurantRequest(user.bearer, dto);

        // assert
        _context.restaurantTable.Count().Should().Be(1);
        _context.restaurantTable.First().Should().BeEquivalentTo(rest, x => x.Excluding(v => v.Id));
        _context.requestTable.Count().Should().Be(1);
    }

    [Fact]
    public async Task CreateCourierRequest_WithCorrectData_CreatesCourierRequest()
    {
        // arrange
        var user = await createUser();
        var car_number = "A785BC64";
        var desc = "some desc";

        // act
        await _sut.CreateCourierRequest(user.bearer, car_number, desc);

        // assert
        _context.courierTable.Count().Should().Be(1);
        _context.courierTable.First().car_number.Should().Be(car_number);
        _context.courierTable.First().userId.Should().Be(user.user.Id);

        _context.requestTable.Count().Should().Be(1);
        _context.requestTable.First().description.Should().Be(desc);
        _context.requestTable.First().user_id.Should().Be(user.user.Id);
    }

    [Fact]
    public async Task CreateOrder_WithCorrectData_AutoAccepting()
    {
        // arrange
        var user = await createUser(money: 600);

        var owner = await createUser();
        var rest = Generator.GenerateRestaurant(owner.user.Id, RestaurantStatus.Verified);
        var food1 = Generator.GenerateFoodItem(rest.Id, 300);
        var basket1 = Generator.GenBasket(user.user.Id, food1.Id);
        var basket2 = Generator.GenBasket(user.user.Id, food1.Id);

        _context.restaurantTable.Add(rest);
        _context.restaurantFoodItemsTable.Add(food1);
        _context.basketTable.AddRange(basket1, basket2);
        _context.SaveChanges();

        _rabbit.QueuePurge("client_to_restaurant");
        await _listener.StartAsync(new CancellationToken());

        // act
        await _sut.CreateOrder(user.bearer);
        await Task.Delay(100); // сон для того чтобы rabbit успел обработать запрос

        // assert
        var order = _context.orderTable.First();
        order.status.Should().Be(OrderStatus.Accepted);
        order.total_price.Should().Be(600);
        order.client_id.Should().Be(user.user.Id);
        order.restaurant_id.Should().Be(rest.Id);

        _context.orderHistory.Count().Should().Be(2);
    }
}

public class RequestInfoCourierComparer : IEqualityComparer<object>
{
    public new bool Equals(object? x, object? y)
    {
        if (x is not RequestInfo_Couriers request || y is not (CourierTable courier, RequestTable reqTable, UserTable user))
            return false;

        return request.car_number == courier.car_number

            && request.request_description == reqTable.description
            && request.request_time_add == reqTable.time_add
            && request.request_id == reqTable.Id

            && request.client_info.Id == user.Id
            && request.client_info.address == user.address
            && request.client_info.chat_id == user.telegram_chat_id
            && request.client_info.username == user.username
            && request.client_info.photo_url == user.photo_url
            && request.client_info.first_name == user.first_name
            && request.client_info.last_name == user.last_name
            && string.Join(" ", request.client_info.roles) == string.Join(" ", user.roles);
    }

    public int GetHashCode([DisallowNull] object obj)
    {
        throw new NotImplementedException();
    }
}

public class RequestInfoRestaurantComparer : IEqualityComparer<object>
{
    public new bool Equals(object? x, object? y)
    {
        if (x is not RequestInfo_Restaurants request || y is not (RestaurantTable rest, RequestTable reqTable, UserTable user))
            return false;

        return request.restaurantName == rest.restaurantName
            && request.address == rest.address
            && request.close_time == rest.close_time
            && request.open_time == rest.open_time
            && request.description == rest.description
            && request.imagePath == rest.imagePath
            && request.phone_number == rest.phone_number
            
            && request.request_description == reqTable.description
            && request.request_time_add == reqTable.time_add
            && request.request_id == reqTable.Id
            
            && request.client_info.Id == user.Id
            && request.client_info.address == user.address
            && request.client_info.chat_id == user.telegram_chat_id
            && request.client_info.username == user.username
            && request.client_info.photo_url == user.photo_url
            && request.client_info.first_name == user.first_name
            && request.client_info.last_name == user.last_name
            && string.Join(" ", request.client_info.roles) == string.Join(" ", user.roles);
    }

    public int GetHashCode([DisallowNull] object obj)
    {
        throw new NotImplementedException();
    }
}

public class UserClientInfoComparer : IEqualityComparer<object>
{
    public new bool Equals(object? x, object? y)
    {
        if (y is not UserTable user || x is not ORM_Components.DTO.ClientAPI.ClientInfo info)
            return false;

        return string.Join(" ", info.roles) == string.Join(" ", user.roles)
            && info.username == user.username
            && info.Id == user.Id
            && info.chat_id == user.telegram_chat_id
            && info.address == user.address
            && info.first_name == user.first_name
            && info.money_value == user.money_value
            && info.telegram_id == user.telegram_id
            && info.photo_url == user.photo_url
            && info.last_name == user.last_name;
    }

    public int GetHashCode([DisallowNull] object obj)
    {
        throw new NotImplementedException();
    }
}
