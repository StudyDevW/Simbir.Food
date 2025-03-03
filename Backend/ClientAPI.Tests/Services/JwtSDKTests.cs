using FluentAssertions;
using Middleware_Components.JWT;
using Middleware_Components.JWT.DTO.CheckUsers;
using System.IdentityModel.Tokens.Jwt;
using TestsBaseLib.Base;
using TestsBaseLib.Mocks;

namespace ClientAPI.Tests.Services;

public class JwtSDKTests
{
    [Fact]
    public void JwtTokenCreation_WithCorrectAuthCheckSuccessDto_ReturnsJwtTokenWithRightClaims()
    {
        // arrange
        var cacheMock = new DictCacheServiceMock();
        var config = TestConfiguration.GetConfiguration();

        var sdk = new JwtSDK(config, cacheMock);

        var dto = new Auth_CheckSuccess
        {
            Id = Guid.NewGuid(),
            roles = new List<string> { "Client", "Admin" },
            telegram_chat_id = 125125664,
            device = "Windows 10 Desktop"
        };

        // act
        var token = sdk.JwtTokenCreation(dto);

        // assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var securityToken = (JwtSecurityToken)tokenHandler.ReadToken(token);

        var claims = securityToken.Claims;

        token.Should().NotBeNull();
        claims.Single(x => x.Type == "iss").Value.Should().Be(config["Jwt:Issuer"]);
        claims.Single(x => x.Type == "aud").Value.Should().Be(config["Jwt:Audience"]);

        claims.Any(x => x.Value == dto.Id.ToString()).Should().BeTrue();

        claims.Any(x => x.Value.Contains("Client") && x.Value.Contains("Admin"))
            .Should().BeTrue();
    }

    [Fact]
    public void RefreshTokenCreation_WithCorrectAuthCheckSuccessDto_ReturnsJwtTokenWithRightClaims()
    {
        // arrange
        var cacheMock = new DictCacheServiceMock();
        var config = TestConfiguration.GetConfiguration();

        var sdk = new JwtSDK(config, cacheMock);

        var dto = new Auth_CheckSuccess
        {
            Id = Guid.NewGuid(),
            roles = new List<string> { "Client", "Admin" },
            telegram_chat_id = 62362376
        };

        // act
        var token = sdk.RefreshTokenCreation(dto);

        // assert
        token.Should().NotBeNull();
    }
}
