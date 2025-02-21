using ClientAPI.Interfaces;
using ClientAPI.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Middleware_Components.DTO.ClientAPI;
using Middleware_Components.Services;
using Moq;
using ORM_Components;
using ORM_Components.DTO.ClientAPI;
using ORM_Components.Tables;
using StackExchange.Redis;
using Telegram_Components.Interfaces;
using TestsBaseLib.Base;

namespace ClientAPI.Tests.Services;

/// <summary>
/// Integration tests of ClientService.
/// For running this tests you need to run redis and postgresql in docker
/// </summary>
public class ClientServiceTests : BaseTest
{
    private readonly IMessageSender _sender;
    private readonly ICacheService _cache;
    private readonly ISessionService _session;
    private readonly DataContext _context;
    private readonly IDatabaseService _database;
    private readonly IJwtService _jwt;
    private readonly IConnectionMultiplexer _multiplexer;

    public ClientServiceTests()
    {
        _sender = Mock.Of<IMessageSender>();
        _multiplexer = GetConnectionMultiplexer();
        _cache = GetCacheService(_multiplexer);
        _session = GetSessionService(_cache);
        _context = GetDbContext();
        _database = GetDataService(_context);
        _jwt = GetJwtService(_cache);
    }

    private async Task ClearDataBeforeTesting()
    {
        // clear redis
        var endpoint = _multiplexer.GetEndPoints().First();
        _multiplexer.GetServer(endpoint).FlushDatabase();

        // clear database
        _context.userTable.RemoveRange(_context.userTable);
        _context.restaurantTable.RemoveRange(_context.restaurantTable);
        _context.courierTable.RemoveRange(_context.courierTable);
        _context.orderTable.RemoveRange(_context.orderTable);
        _context.orderItemsTable.RemoveRange(_context.orderItemsTable);
        _context.reviewTable.RemoveRange(_context.reviewTable);
        _context.restaurantFoodItemsTable.RemoveRange(_context.restaurantFoodItemsTable);
        await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task RegisterUser_WithCorrectData_ReturnsTokens()
    {
        // arrange
        await ClearDataBeforeTesting();

        var clientService = new ClientService(_sender, _session, _database, _jwt, _cache);

        var dto = new AuthSignUp
        {
            login = "test1",
            password = "pass1",
            address = "Moscow",
            email = "mis@gmail.com",
            name = "dominik",
            phone_number = "7590235",
            telegram_chatid = "182905126"
        };

        // act
        var result = await clientService.RegisterUser(dto);

        // assert
        result.Should().NotBeNull();
        result.accessToken.Should().NotBeNull();
        result.refreshToken.Should().NotBeNull();

        var user = await _context.userTable.FirstOrDefaultAsync(x => x.login == dto.login);
        user.Should().NotBeNull();

        var db = _multiplexer.GetDatabase();
        var session = db.StringGet($"session_storage_storage_{user.Id}");
        var access = db.StringGet($"accessTokens_storage_{user.Id}");
        var refresh = db.StringGet($"refreshTokens_storage_{user.Id}");

        session.HasValue.Should().BeTrue();
        access.HasValue.Should().BeTrue();
        refresh.HasValue.Should().BeTrue();
    }

    [Fact]
    public async Task LoginClient_WithCorrectData_ReturnsTokens()
    {
        // arrange
        await ClearDataBeforeTesting();

        var clientService = new ClientService(_sender, _session, _database, _jwt, _cache);

        var login = "test1";
        var password = "pass1";

        var dto = new AuthSignIn
        {
            login = login,
            password = password,
            telegram_chatid = "1241285295"
        };

        var addedUser = await AuthNewUser(login, password);

        // act
        var result = clientService.LoginClient(dto);

        // assert
        result.Should().NotBeNull();
        result.accessToken.Should().NotBeNull();
        result.refreshToken.Should().NotBeNull();

        var db = _multiplexer.GetDatabase();
        var session = db.StringGet($"session_storage_storage_{addedUser.user.Id}");
        var access = db.StringGet($"accessTokens_storage_{addedUser.user.Id}");
        var refresh = db.StringGet($"refreshTokens_storage_{addedUser.user.Id}");

        session.HasValue.Should().BeTrue();
        access.HasValue.Should().BeTrue();
        refresh.HasValue.Should().BeTrue();
    }

    [Fact]
    public async Task ClientSignOut_WithCorrectBearerKey_ReturnsSuccessString()
    {
        // arrange
        await ClearDataBeforeTesting();

        var clientService = new ClientService(_sender, _session, _database, _jwt, _cache);

        var login = "test1";
        var password = "pass1";

        var addedUser = await AuthNewUser(login, password);

        // act
        var result = await clientService.ClientSignOut("Bearer " + addedUser.tokens.accessToken);

        // assert
        result.Should().NotBeNull();

        var db = _multiplexer.GetDatabase();
        var session = db.StringGet($"session_storage_storage_{addedUser.user.Id}");
        var access = db.StringGet($"accessTokens_storage_{addedUser.user.Id}");
        var refresh = db.StringGet($"refreshTokens_storage_{addedUser.user.Id}");

        session.HasValue.Should().BeTrue();
        access.HasValue.Should().BeFalse();
        refresh.HasValue.Should().BeFalse();
    }

    [Fact]
    public async Task RefreshClientSession_WithCorrectData_ReturnsTokens()
    {
        // arrange
        await ClearDataBeforeTesting();

        var clientService = new ClientService(_sender, _session, _database, _jwt, _cache);

        var login = "test1";
        var password = "pass1";

        var addedUser = await AuthNewUser(login, password);
        var tokens = addedUser.tokens;

        // act
        var result = await clientService.RefreshClientSession(new Auth_RefreshTokens
        {
            refreshToken = tokens.refreshToken
        });

        // assert
        result.Should().NotBeNull();
        result.accessToken.Should().NotBeNull();
        result.refreshToken.Should().NotBeNull();

        tokens.accessToken.Should().NotBe(result.accessToken);
        tokens.refreshToken.Should().NotBe(result.refreshToken);

        var db = _multiplexer.GetDatabase();
        var session = db.StringGet($"session_storage_storage_{addedUser.user.Id}");
        var access = db.StringGet($"accessTokens_storage_{addedUser.user.Id}");
        var refresh = db.StringGet($"refreshTokens_storage_{addedUser.user.Id}");

        session.HasValue.Should().BeTrue();
        access.HasValue.Should().BeTrue();
        refresh.HasValue.Should().BeTrue();
    }

    [Fact]
    public async Task ClientMeInfo_WithCorrectBearerKey_ReturnsClientInfo()
    {
        // arrange
        await ClearDataBeforeTesting();

        var clientService = new ClientService(_sender, _session, _database, _jwt, _cache);

        var login = "test1";
        var password = "pass1";

        var addedUser = await AuthNewUser(login, password);
        var tokens = addedUser.tokens;
        var user = addedUser.user;
        var bearerKey = "Bearer " + tokens.accessToken;

        // act
        var result = await clientService.ClientMeInfo(bearerKey);

        // assert
        result.Should().NotBeNull();
        result.Id.Should().Be(user.Id);
        result.login.Should().Be(user.login);
    }

    [Fact]
    public async Task UpdateClientInfo_WithCorrectData_UpdatesClient()
    {
        // arrange
        await ClearDataBeforeTesting();

        var clientService = new ClientService(_sender, _session, _database, _jwt, _cache);

        var login = "test1";
        var password = "pass1";

        var addedUser = await AuthNewUser(login, password);
        var tokens = addedUser.tokens;
        var user = addedUser.user;
        var bearerKey = "Bearer " + tokens.accessToken;

        var dto = new ClientUpdate
        {
            address = "Omsk",
            avatarImage = "other image",
            email = "other@mail.ru",
            name = "othername",
            password = "newpass1",
            phone_number = "89532053253"
        };

        // act
        await clientService.UpdateClientInfo(bearerKey, dto);

        // assert

        var updatedUser = await _context.userTable.FindAsync(user.Id);
        updatedUser.Should().NotBeNull();
        updatedUser.address.Should().Be(dto.address);
        updatedUser.avatarImage.Should().Be(dto.avatarImage);
        updatedUser.email.Should().Be(dto.email);
        updatedUser.name.Should().Be(dto.name);
        updatedUser.phone_number.Should().Be(dto.phone_number);

        var hasher = new PasswordHasher<PasswordAppUser>();
        var verify = hasher.VerifyHashedPassword(new PasswordAppUser { login = updatedUser.login, passwordHashed = updatedUser.password },
            updatedUser.password, dto.password);

        verify.Should().Be(PasswordVerificationResult.Success);
    }

    [Fact]
    public async Task AllProfilesGet_WithCorrectData_ReturnsAllProfiles()
    {
        // arrange
        await ClearDataBeforeTesting();

        var clientService = new ClientService(_sender, _session, _database, _jwt, _cache);

        var login = "test1";
        var password = "pass1";

        var addedUser = await AuthNewUser(login, password, "Admin");
        var tokens = addedUser.tokens;
        var user = addedUser.user;
        var bearerKey = "Bearer " + tokens.accessToken;

        for (int i = 1; i < 9; i++)
            _context.userTable.Add(new UserTable
            {
                Id = Guid.NewGuid(),
                login = $"user_{i}",
                address = "testadd",
                email = "test@gmail.com",
                name = "test",
                phone_number = "7590235",
                roles = new string[] { "Client" },
                password = "hash"
            });

        await _context.SaveChangesAsync();

        // act
        var clients = await clientService.AllProfilesGet(bearerKey, 2, 5);

        // assert
        clients.Should().NotBeNull();
        clients.Content.Count.Should().Be(5);
    }

    [Fact]
    public async Task CreateClientWithAdmin_WithCorrectData_AddsUserToDb()
    {
        // arrange
        await ClearDataBeforeTesting();

        var clientService = new ClientService(_sender, _session, _database, _jwt, _cache);

        var dto = new ClientAdd_Admin
        {
            login = "newuser1",
            password = "newuser1pass",
            address = "Ufa",
            email = "ufen@gmail.com",
            name = "ufenic",
            phone_number = "52363272",
            telegram_chatid = "734834834",
            roles = new string[] { "Client", "Admin" }
        };

        var addedAdmin = await AuthNewUser("admin1", "pass1", "Admin");
        var bearer = "Bearer " + addedAdmin.tokens.accessToken;

        // act
        await clientService.CreateClientWithAdmin(bearer, dto);

        // assert
        var user = await _context.userTable.FirstOrDefaultAsync(x => x.login == dto.login);
        user.roles.Should().BeEquivalentTo(dto.roles);
        user.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteClientWithAdmin_WithCorrectData_DeletesUser()
    {
        // arrange
        await ClearDataBeforeTesting();

        var clientService = new ClientService(_sender, _session, _database, _jwt, _cache);

        var addedAdmin = await AuthNewUser("admin1", "pass1", "Admin");
        var bearer = "Bearer " + addedAdmin.tokens.accessToken;

        var deleteableUser = await AddUserToDb("deluser1", "delpass1");

        var restaurant = GenerateRestaurant(deleteableUser.Id);
        var foods = GenerateFoodItems(restaurant.Id, 3);

        _context.restaurantTable.Add(restaurant);
        _context.restaurantFoodItemsTable.AddRange(foods);
        await _context.SaveChangesAsync();

        // act
        await clientService.DeleteClientWithAdmin(bearer, deleteableUser.Id);

        // assert
        var user = await _context.userTable.FindAsync(deleteableUser.Id);
        user.Should().BeNull();

        var restaurantsLeft = await _context.restaurantTable.CountAsync();
        restaurantsLeft.Should().Be(0);

        var foodsLeft = await _context.restaurantFoodItemsTable.CountAsync();
        foodsLeft.Should().Be(0);
    }
}
