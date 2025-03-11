using ClientAPI.Interfaces;
using ClientAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Middleware_Components.Broker;
using Middleware_Components.Cache;
using Middleware_Components.DTO.ClientAPI;
using Middleware_Components.JWT;
using Middleware_Components.JWT.DTO.CheckUsers;
using Middleware_Components.Services;
using Moq;
using ORM_Components;
using ORM_Components.Tables;
using RabbitMQ.Client;
using StackExchange.Redis;
using System.Text.Json;
using Telegram.Bot;
using Telegram_Components.Interfaces;
using Telegram_Components.Services;

namespace TestsBaseLib.Base;
public class IntegrationTest
{
    protected IConfiguration Configuration { get; set; }
    public IntegrationTest()
    {
        Configuration = TestConfiguration.GetConfiguration();
    }

    protected DataContext GetDbContext()
    {
        var builder = new DbContextOptionsBuilder<DataContext>()
            .UseNpgsql(Configuration["DatabaseConnectionString"]!);

        var context = new DataContext(builder.Options);
        context.Database.EnsureCreated();

        return context;
    }

    protected void ClearDatabase(DataContext context)
    {
        context.userTable.RemoveRange(context.userTable);
        context.bankCardTable.RemoveRange(context.bankCardTable);
        context.basketTable.RemoveRange(context.basketTable);
        context.cardUsersTable.RemoveRange(context.cardUsersTable);
        context.courierTable.RemoveRange(context.courierTable);
        context.orderHistory.RemoveRange(context.orderHistory);
        context.orderItemsTable.RemoveRange(context.orderItemsTable);
        context.orderTable.RemoveRange(context.orderTable);
        context.payTable.RemoveRange(context.payTable);
        context.requestTable.RemoveRange(context.requestTable);
        context.restaurantFoodItemsTable.RemoveRange(context.restaurantFoodItemsTable);
        context.restaurantTable.RemoveRange(context.restaurantTable);
        context.reviewTable.RemoveRange(context.reviewTable);
        context.userTable.RemoveRange(context.userTable);

        context.SaveChanges();
    }

    protected void ClearRedis()
    {
        var mult = GetConnectionMultiplexer();
        var server = mult.GetServer(Configuration["RedisEndPoint"]!);
        server.FlushDatabase();
    }

    protected IConnectionMultiplexer GetConnectionMultiplexer()
    {
        var redis = ConnectionMultiplexer.Connect(
                new ConfigurationOptions
                {
                    EndPoints = { Configuration["RedisEndPoint"]! },
                    Password = Configuration["RedisPassword"]!,
                    AbortOnConnectFail = false,
                    AllowAdmin = true
                });

        return redis;
    }

    protected ICacheService GetCacheService(IConnectionMultiplexer multiplexer)
    {
        var db = multiplexer.GetDatabase();

        return new CacheSDK(db);
    }
    protected ISessionService GetSessionService(ICacheService cache)
    {
        return new SessionService(cache);
    }

    protected ITelegramBotClient GetBotClient() => Mock.Of<ITelegramBotClient>();

    protected IDatabaseOperations GetOperations(DataContext context) => new DatabaseOperations(context);

    protected IMessageReceiver GetTelegramMessageReceiver(ITelegramBotClient client, IDatabaseOperations oper, ICacheService cache)
    {
        return new MessageReceiver(client, oper, cache);
    }

    protected IRabbitMQService GetRabbitService()
    {
        var factory = new ConnectionFactory
        {
            HostName = Configuration["Rabbit:Hostname"]!,
            UserName = Configuration["Rabbit:Username"]!,
            Password = Configuration["Rabbit:Password"]!,
            Port = int.Parse(Configuration["Rabbit:Port"]!)
        };

        return new RabbitMQService(factory);
    }

    protected IDatabaseService GetDataService(DataContext context)
    {
        return new DatabaseService(context);
    }

    protected IJwtService GetJwtService(ICacheService cache)
    {
        return new JwtSDK(Configuration, cache);
    }

    /// <summary>
    /// !!! Только для интеграционных тестов !!!
    /// Добавляет нового пользователя в БД
    /// </summary>
    protected async Task<UserTable> AddUserToDb(string role = "Client")
    {
        var user = Generator.GenerateUser(role);

        using (var _context = GetDbContext())
        {
            _context.userTable.Add(user);
            await _context.SaveChangesAsync();
        }

        return user;
    }

    /// <summary>
    /// !!! Только для интеграционных тестов !!!
    /// Добавляет нового пользователя в БД и авторизует его, добавляя токены в кэш Redis
    /// </summary>
    protected async Task<(UserTable user, Auth_PairTokens tokens)> AuthNewUser(string role = "Client")
    {
        var user = await AddUserToDb(role);

        var check = new Auth_CheckSuccess
        {
            Id = user.Id,
            telegram_chat_id = user.telegram_chat_id
        };

        using (var _multiplexer = GetConnectionMultiplexer())
        {
            var _cache = GetCacheService(_multiplexer);
            var _jwt = GetJwtService(_cache);

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
    }
}
