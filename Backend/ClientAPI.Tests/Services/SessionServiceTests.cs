using ClientAPI.Services;
using FluentAssertions;
using Middleware_Components.DTO.ClientAPI;
using ORM_Components.Tables;
using System.Text.Json;
using TestsBaseLib.Mocks;

namespace ClientAPI.Tests.Services;

public class SessionServiceTests
{
    [Fact]
    public void SetupSession_WithCorrectDataAndFirstTimeCall_CreatesCacheSession()
    {
        // arrange
        var cache = new DictCacheServiceMock();
        var sut = new SessionService(cache);

        var userId = Guid.NewGuid();
        var token = "some_token";

        // act
        sut.SetupSession(userId, token);

        // assert
        var stringValue = cache.Dict[$"session_storage_storage_{userId}"];
        var value = JsonSerializer.Deserialize<Session_Init[]>(stringValue);

        value.Should().NotBeNull();
        value.Count().Should().Be(1);
        value.First().tokenSession.Should().Be(token);
   }

    [Fact]
    public void SetupSession_WithCorrectDataAndSecondTimeCall_AddedTokenToOldCacheSession()
    {
        // arrange
        var cache = new DictCacheServiceMock();
        var sut = new SessionService(cache);

        var userId = Guid.NewGuid();
        var token = "some_token";
        var second_token = "second_token";

        // act
        sut.SetupSession(userId, token);
        sut.SetupSession(userId, second_token);

        // assert
        var stringValue = cache.Dict[$"session_storage_storage_{userId}"];
        var value = JsonSerializer.Deserialize<Session_Init[]>(stringValue);

        value.Should().NotBeNull();
        value.Count().Should().Be(2);
        value.First().tokenSession.Should().Be(token);
        value.Last().tokenSession.Should().Be(second_token);

        value.First().statusSession.Should().Be("expired");
        value.Last().statusSession.Should().Be("active");
        value.First().timeDel.Value.Should().BeLessThan(TimeSpan.FromSeconds(1)).Before(DateTime.UtcNow);
    }

    [Fact]
    public void ClientSignOutSession_WithCorrectData_AllActiveSessionsHaveToBeExpired()
    {
        // arrange
        var cache = new DictCacheServiceMock();
        var sut = new SessionService(cache);

        var userId = Guid.NewGuid();
        var token = "some_token";

        // act
        sut.SetupSession(userId, token);
        sut.ClientSignOutSession(userId);

        // assert
        var stringValue = cache.Dict[$"session_storage_storage_{userId}"];
        var value = JsonSerializer.Deserialize<Session_Init[]>(stringValue);

        value.All(x => x.statusSession == "expired").Should().BeTrue();
    }

    [Fact]
    public void RefreshSession_WithCorrectDataAndSecondTimeCall_AddedTokenToOldCacheSession()
    {
        // arrange
        var cache = new DictCacheServiceMock();
        var sut = new SessionService(cache);

        var userId = Guid.NewGuid();
        var token = "some_token";
        var new_token = "second_token";

        // act
        sut.SetupSession(userId, token);
        sut.RefreshSession(userId, new_token);

        // assert
        var stringValue = cache.Dict[$"session_storage_storage_{userId}"];
        var value = JsonSerializer.Deserialize<Session_Init[]>(stringValue);

        value.First().timeUpd.Value.Should().BeLessThan(TimeSpan.FromSeconds(1)).Before(DateTime.UtcNow);
        value.First().tokenSession.Should().Be(new_token);
    }
}
