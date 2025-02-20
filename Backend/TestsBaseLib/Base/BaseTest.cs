using ClientAPI.Interfaces;
using ClientAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Middleware_Components.Cache;
using Middleware_Components.JWT;
using Middleware_Components.Services;
using Moq;
using ORM_Components;
using StackExchange.Redis;

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
}
