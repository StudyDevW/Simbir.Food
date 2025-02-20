using ClientAPI.Interfaces;
using ClientAPI.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Middleware_Components.Services;
using Moq;
using ORM_Components;
using ORM_Components.DTO.ClientAPI;
using StackExchange.Redis;
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
    }
}
