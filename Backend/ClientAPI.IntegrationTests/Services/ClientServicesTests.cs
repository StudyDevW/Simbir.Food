using ClientAPI.Interfaces;
using ClientAPI.Services;
using FluentAssertions;
using Middleware_Components.DTO.ClientAPI;
using Middleware_Components.Services;
using Moq;
using ORM_Components;
using ORM_Components.DTO.ClientAPI;
using ORM_Components.Tables;
using ORM_Components.Tables.Helpers;
using StackExchange.Redis;
using System.Diagnostics.CodeAnalysis;
using Telegram_Components.Interfaces;
using TestsBaseLib.Base;

namespace ClientAPI.IntegrationTests.Services;

public class ClientServicesTests : IntegrationTest
{
    private readonly ClientService _sut;
    private readonly RabbitMQListenerService _listener;
    private readonly DataContext _context;
    private readonly ICacheService _cache;
    private readonly IDatabase _cacheDatabase;
    private readonly ISessionService _session;

    public ClientServicesTests()
    {
        var rabbit = GetRabbitService();
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

        _listener = new ClientAPI.Services.RabbitMQListenerService(database, tgsender, rabbit);

        _sut = new ClientService(rabbit, tgsender, session, database, jwt, cache);

        ClearDatabase(context);
        //ClearRedis();
    }

    /// <summary>
    /// Данный метод авторизирует нового пользователя и возвращает пользователя + Bearer токен
    /// </summary>
    private async Task<(UserTable user, string bearer, string refresh, string access)> createUser(string roles = "Client")
    {
        var user = Generator.GenerateUser("Admin");

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
