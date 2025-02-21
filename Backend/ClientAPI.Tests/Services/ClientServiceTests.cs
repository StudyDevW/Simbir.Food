using ClientAPI.Interfaces;
using ClientAPI.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Middleware_Components.DTO.ClientAPI;
using Middleware_Components.JWT.DTO.CheckUsers;
using Middleware_Components.Services;
using Moq;
using ORM_Components;
using ORM_Components.DTO.ClientAPI;
using ORM_Components.Tables;
using StackExchange.Redis;
using System.Text.Json;
using Telegram.Bot.Types;
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

    private async Task<(UserTable user, Auth_PairTokens tokens)> AddUser(string login, string password)
    {
        var hasher = new PasswordHasher<PasswordAppUser>();

        var user = new UserTable
        {
            login = login,
            password = hasher.HashPassword(new PasswordAppUser { login = login }, password),
            address = "Moscow",
            email = "mis@gmail.com",
            name = "dominik",
            phone_number = "7590235",
            roles = new string[] { "Client" },
        };

        _context.userTable.Add(user);
        await _context.SaveChangesAsync();

        var check = new Auth_CheckSuccess
        {
            Id = user.Id,
            login = user.login,
            roles = user.roles.ToList(),
            telegramChatId = "958235235"
        };

        var db = _multiplexer.GetDatabase();

        var accessToken = _jwt.JwtTokenCreation(check);
        var refreshToken = _jwt.RefreshTokenCreation(check);

        db.StringSet($"accessTokens_storage_{check.Id}", JsonSerializer.Serialize(accessToken), TimeSpan.FromMinutes(5));
        db.StringSet($"refreshTokens_storage_{check.Id}", JsonSerializer.Serialize(refreshToken), TimeSpan.FromDays(7));

        var sessionInit = new List<Session_Init>()
        {
            new Session_Init()
            {
                timeAdd = DateTime.UtcNow,
                statusSession = "active",
                tokenSession = accessToken
            }
        };

        db.StringSet($"session_storage_storage_{check.Id}", JsonSerializer.Serialize(sessionInit), TimeSpan.FromDays(7));

        return (user, new Auth_PairTokens
        {
            accessToken = accessToken,
            refreshToken = refreshToken
        });
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

        var addedUser = await AddUser(login, password);

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

        var addedUser = await AddUser(login, password);

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

        var addedUser = await AddUser(login, password);
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
}
