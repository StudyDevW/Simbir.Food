using ClientAPI.Services;
using FluentAssertions;
using Middleware_Components.Broker;
using Middleware_Components.Services;
using Moq;
using ORM_Components;
using ORM_Components.DTO.ClientAPI;
using RabbitMQ.Client;
using StackExchange.Redis;
using System.Text.Json;
using Telegram.Bot.Types;
using Telegram_Components.Interfaces;
using Telegram_Components.Services;
using TestsBaseLib.Base;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ClientAPI.IntegrationTests.Services;

public class ClientServicesTests : IntegrationTest
{
    private readonly ClientService _sut;
    private readonly RabbitMQListenerService _listener;
    private readonly DataContext _context;
    private readonly ICacheService _cache;
    private readonly IDatabase _cacheDatabase;

    public ClientServicesTests()
    {
        var rabbit = GetRabbitService();
        var tgsender = Mock.Of<IMessageSender>();

        var multiplexer = GetConnectionMultiplexer();
        _cacheDatabase = multiplexer.GetDatabase();

        var cache = GetCacheService(multiplexer);
        _cache = cache;

        var session = GetSessionService(cache);
        var context = GetDbContext();
        _context = context;

        var database = GetDataService(context);
        var jwt = GetJwtService(cache);

        _listener = new ClientAPI.Services.RabbitMQListenerService(database, tgsender, rabbit);

        _sut = new ClientService(rabbit, tgsender, session, database, jwt, cache);

        ClearDatabase(context);
        //ClearRedis();
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
    }
}
