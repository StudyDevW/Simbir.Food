using Bogus;
using ClientAPI.Interfaces;
using ClientAPI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Middleware_Components.Cache;
using Middleware_Components.DTO.ClientAPI;
using Middleware_Components.JWT;
using Middleware_Components.JWT.DTO.CheckUsers;
using Middleware_Components.Services;
using Moq;
using ORM_Components;
using ORM_Components.DTO.ClientAPI;
using ORM_Components.Tables;
using StackExchange.Redis;
using System.Text.Json;

namespace TestsBaseLib.Base;

public class BaseTest
{
    protected DataContext GetDbContext()
    {
        var builder = new DbContextOptionsBuilder<DataContext>()
            .UseNpgsql("Server=localhost;Port=5432;User Id=postgres;Password=admin;Database=simbirtests");

        var context = new DataContext(builder.Options);
        context.Database.Migrate();

        return context;
    }

    protected IConnectionMultiplexer GetConnectionMultiplexer()
    {
        var redis = ConnectionMultiplexer.Connect(
                new ConfigurationOptions
                {
                    EndPoints = { "localhost:6379" },
                    Password = "root",
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

    protected IDatabaseService GetDataService(DataContext context)
    {
        return new DatabaseService(context);
    }

    protected IJwtService GetJwtService(ICacheService cache)
    {
        var conf = new Mock<IConfiguration>();
        conf.Setup(x => x["RSA_PUBLIC_KEY"]).Returns("-----BEGIN PUBLIC KEY-----\nMIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCeXVuaJGOQleufBCm+80wkZwEI\n34QhypgPc1rFpAJqq6NLDNVuHbKmqE8qejzg5VAHnkTJk4c+3urwITUKtc+z1/VO\nCPbR+GeIdX65k9EH8YolRnwe972vqNYd4NiTKRSFSldsdSw2yUj0grwALkSir4Ja\niBD4EakJ4NrkWdb2+wIDAQAB\n-----END PUBLIC KEY-----");
        conf.Setup(x => x["RSA_PRIVATE_KEY"]).Returns("-----BEGIN RSA PRIVATE KEY-----\nMIICXAIBAAKBgQCeXVuaJGOQleufBCm+80wkZwEI34QhypgPc1rFpAJqq6NLDNVu\nHbKmqE8qejzg5VAHnkTJk4c+3urwITUKtc+z1/VOCPbR+GeIdX65k9EH8YolRnwe\n972vqNYd4NiTKRSFSldsdSw2yUj0grwALkSir4JaiBD4EakJ4NrkWdb2+wIDAQAB\nAoGAFbHQZLNreFkxaB1X4rLN0YbS23ZTUZXBcwxoeP7Y3egZfKSLcIRc/vu7rKQG\nRwDjD8gcwEiXlINRSAgkjg0OIOt/1AsbGsBdZhh2NCWWCLumeKuyGB+6xWAm9Qb8\noIxzBmkHdgx1ykLsTQH6cakoX51slTJ7tdbFM00fQBswLyECQQDiRGbhZZYEkEk6\nCeTJimw/iQWGXtLVV6T6ck0eRrpG8m3FHQTxEqPOHEQFK6iMJEocjjSZ6tm3Es6q\nxu2/4LQJAkEAsyy5csFBjnCybvHDSdfLCa33tjoLeCTFFJM6uF8a8swuyVLWuTKl\nYYY5zoCreeuhO92pSwWpCG+8MzX5Yh974wJARIPb92Kwg59BXT7Dtbehwbd3IdIy\n24FXprLX4VQfcf5U+Pwpk+pGCdKLUll/Bzix7GWvTfBMjuA2DoaAVbrwKQJBAIQU\naRRt39yXuQFN2O77U2HsS1maik/jkyBqs/OrsBrhZ2/jUAQvkHhG0SAn+8AhcbbG\n3QA/yO4+J9b8Z7zsho8CQHVULz1iy2uHeKZawAZqiME10Uy0OEmswV3Yot18hLhe\n+kAUrjGyT6o5p2SRh5yYpQ5pANC9yK3CQ1wGr97z0yw=\n-----END RSA PRIVATE KEY-----");
        conf.Setup(x => x["Jwt:Issuer"]).Returns("SimbirFood");
        conf.Setup(x => x["Jwt:Audience"]).Returns("SimbirFoodUsers");

        return new JwtSDK(conf.Object, cache);
    }

    protected UserTable GenerateUser(string login, string passwordHash, string[] roles)
    {
        var faker = new Faker<UserTable>();
        faker.RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.login, f => login)
            .RuleFor(x => x.password, _ => passwordHash)
            .RuleFor(x => x.name, f => f.Name.FirstName())
            .RuleFor(x => x.email, (f, x) => f.Internet.Email(x.name))
            .RuleFor(x => x.phone_number, f => f.Phone.PhoneNumber())
            .RuleFor(x => x.address, f => f.Address.City())
            .RuleFor(x => x.roles, _ => roles);

        return faker.Generate();
    }

    protected RestaurantTable GenerateRestaurant(Guid user_id)
    {
        var faker = new Faker<RestaurantTable>();
        faker.RuleFor(x => x.Id, _ => Guid.NewGuid())
            .RuleFor(x => x.user_id, _ => user_id)
            .RuleFor(x => x.phone_number, f => f.Phone.PhoneNumber())
            .RuleFor(x => x.imagePath, f => f.Random.Word())
            .RuleFor(x => x.restaurantName, f => f.Random.Word())
            .RuleFor(x => x.description, f => f.Random.Words(10))
            .RuleFor(x => x.address, f => f.Address.City())
            .RuleFor(x => x.close_time, f => f.Date.Between(new DateTime(2016, 1, 1, 0, 0, 0, DateTimeKind.Utc), DateTime.UtcNow))
            .RuleFor(x => x.open_time, (f, x) => f.Date.Between(DateTime.UtcNow, x.close_time))
            .RuleFor(x => x.status, f => f.Random.Word());

        return faker.Generate();
    }

    protected List<RestaurantFoodItemsTable> GenerateFoodItems(Guid restaurant_id, int count)
    {
        var faker = new Faker<RestaurantFoodItemsTable>();
        faker.RuleFor(x => x.Id, _ => Guid.NewGuid())
            .RuleFor(x => x.restaurant_id, _ => restaurant_id)
            .RuleFor(x => x.price, f => f.Random.Number(200))
            .RuleFor(x => x.weight, f => f.Random.Number(1, 100))
            .RuleFor(x => x.calories, f => f.Random.Number(50, 3000))
            .RuleFor(x => x.image, f => f.Random.Word())
            .RuleFor(x => x.name, f => f.Random.Word());

        return faker.Generate(count);
    }

    /// <summary>
    /// !!! Только для интеграционных тестов !!!
    /// Добавляет нового пользователя в БД
    /// </summary>
    protected async Task<UserTable> AddUserToDb(string login, string password, string role = "Client")
    {
        var hasher = new PasswordHasher<PasswordAppUser>();

        var hash = hasher.HashPassword(new PasswordAppUser { login = login }, password);
        var user = GenerateUser(login, hash, new string[] { role });

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
    protected async Task<(UserTable user, Auth_PairTokens tokens)> AuthNewUser(string login, string password, string role = "Client")
    {
        var user = await AddUserToDb(login, password, role);

        var check = new Auth_CheckSuccess
        {
            Id = user.Id,
            login = user.login,
            roles = user.roles.ToList(),
            telegramChatId = "958235235"
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
