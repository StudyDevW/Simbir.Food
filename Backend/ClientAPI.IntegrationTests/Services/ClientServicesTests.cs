using ClientAPI.Services;
using Middleware_Components.Broker;
using Moq;
using ORM_Components.DTO.ClientAPI;
using Telegram_Components.Interfaces;
using TestsBaseLib.Base;

namespace ClientAPI.IntegrationTests.Services;

public class ClientServicesTests : IntegrationTest
{
    private readonly ClientService _sut;

    public ClientServicesTests()
    {
        var rabbit = GetRabbitService();
        var tgsender = Mock.Of<IMessageSender>();

        var multiplexer = GetConnectionMultiplexer();
        var cache = GetCacheService(multiplexer);

        var session = GetSessionService(cache);
        var context = GetDbContext();
        var database = GetDataService(context);
        var jwt = GetJwtService(cache);

        _sut = new ClientService(rabbit, tgsender, session, database, jwt, cache);
    }

    [Fact]
    public async Task UserRegister_WithCorrectData_CreatesNewUser()
    {
        // arrange
        var user = Generator.GenerateUser();

        // act
        //await _sut.UserRegister();

        // assert
    }
}
