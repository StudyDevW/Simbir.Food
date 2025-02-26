using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Middleware_Components.JWT.DTO.Token;
using Middleware_Components.Services;
using Moq;
using Moq.EntityFrameworkCore;
using ORM_Components;
using ORM_Components.DTO.RestaurantAPI;
using ORM_Components.Tables;
using RestaurantAPI.Model.Controllers;
using StackExchange.Redis;
using Telegram.Bot.Types;
using TestsBaseLib.Base;

namespace RestaurantAPI.Tests.Controllers;

public class RestaurantControllerTests
{
    private readonly Mock<DataContext> _context;
    private readonly Mock<IJwtService> _jwt;
    private readonly List<RestaurantTable> _rests;

    private readonly RestaurantController _sut;

    public RestaurantControllerTests()
    {
        _context = new Mock<DataContext>();
        _jwt = new Mock<IJwtService>();

        _rests = new List<RestaurantTable>();

        _context.Setup(x => x.restaurantTable).ReturnsDbSet(_rests);
        _context.Setup(x => x.restaurantTable.Add(It.IsAny<RestaurantTable>()))
            .Callback<RestaurantTable>(x => _rests.Add(x));
        _context.Setup(x => x.restaurantTable.Remove(It.IsAny<RestaurantTable>()))
            .Callback<RestaurantTable>(x => _rests.Remove(x));
        _context.Setup(x => x.restaurantTable.FindAsync(It.IsAny<object>()))
            .ReturnsFindAsync(_rests);

        _sut = new RestaurantController(_context.Object, _jwt.Object);
        _sut.ConfigureContext();
    }

    [Fact]
    public async Task AddRestaurant_WithCorrectData_ReturnsSuccessResult()
    {
        // arrange
        var rest = Generator.GenerateRestaurant(Guid.NewGuid());
        var dto = rest.ToDto();

        _jwt.InitJwt("98423523", "Client");

        // act
        var result = await _sut.AddRestaurant(dto);

        // assert
        result.Should().BeOfType<OkObjectResult>();

        rest.Id = _rests.First().Id;
        _rests.First().Should().BeEquivalentTo(rest);
    }

    [Fact]
    public async Task AddRestaurant_WithNullDto_ReturnsBadRequest()
    {
        // arrange
        Restaurants_DTO dto = null;

        _jwt.InitJwt("98423523", "Client");

        // act
        var result = await _sut.AddRestaurant(dto);

        // assert
        result.Should().BeOfType<BadRequestObjectResult>();
        _rests.Count.Should().Be(0);
    }

    [Theory]
    [InlineData("Moscow", "desc", "imagepath", "859325325", 
        "", "status", "d0d70f16-62f7-4ea3-ad39-b23dd99e204b", "Client")]
    [InlineData("Moscow", "desc", "imagepath", "",
        "Miruko", "status", "d0d70f16-62f7-4ea3-ad39-b23dd99e204b", "Client")]
    [InlineData("", "desc", "imagepath", "859325325",
        "Miruko", "status", "d0d70f16-62f7-4ea3-ad39-b23dd99e204b", "Client")]
    public async Task AddRestaurant_WithWrongData_ReturnsBadRequest(
        string address,
        string description,
        string imagePath,
        string phone_number,
        string restaurantName,
        string status,
        Guid user_id,
        string roles)
    {
        // arrange
        var dto = new Restaurants_DTO
        {
            address = address,
            close_time = DateTime.UtcNow.AddMinutes(+50),
            description = description,
            imagePath = imagePath,
            open_time = DateTime.UtcNow.AddMinutes(-50),
            phone_number = phone_number,
            restaurantName = restaurantName,
            status = status,
            user_id = user_id,
        };

        _jwt.InitJwt("98423523", roles);

        // act
        var result = await _sut.AddRestaurant(dto);

        // assert
        result.Should().BeOfType<BadRequestObjectResult>();
        _rests.Count.Should().Be(0);
    }

    [Theory]
    [InlineData("Client Admin")]
    [InlineData("Client Courier")]
    public async Task AddRestaurant_WithWrongRoles_ReturnsBadRequest(string role)
    {
        // arrange
        var rest = Generator.GenerateRestaurant(Guid.NewGuid());
        var dto = rest.ToDto();

        _jwt.InitJwt("98423523", role);

        // act
        var result = await _sut.AddRestaurant(dto);

        // assert
        result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task DeleteRestaurant_WithCorrectRestId_ReturnsSuccessResult()
    {
        // arrange
        var rest = Generator.GenerateRestaurant(Guid.NewGuid());
        _rests.Add(rest);

        _jwt.InitJwt("98423523", "Client");

        // act
        var result = await _sut.DeleteRestaurant(rest.Id);

        // assert
        result.Should().BeOfType<OkObjectResult>();
        _rests.Count.Should().Be(0);
    }

    [Fact]
    public async Task DeleteRestaurant_WithWrongRestId_ReturnsBadRequest()
    {
        // arrange
        var id = Guid.NewGuid();

        _jwt.InitJwt("98423523", "Client");

        // act
        var result = await _sut.DeleteRestaurant(id);

        // assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetRestaurantById_WithCorrectRestId_ReturnsSuccessResult()
    {
        // arrange
        var rest = Generator.GenerateRestaurant(Guid.NewGuid());
        _rests.Add(rest);

        _jwt.InitJwt("98423523", "Client");

        // act
        var result = await _sut.GetRestaurantById(rest.Id);

        // assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (result as OkObjectResult);
        var table = (okResult.Value as RestaurantTable);
        table.Should().BeEquivalentTo(rest);
    }

    [Fact]
    public async Task GetRestaurantById_WithWrongRestId_ReturnsBadRequest()
    {
        // arrange
        var id = Guid.NewGuid();

        _jwt.InitJwt("98423523", "Client");

        // act
        var result = await _sut.GetRestaurantById(id);

        // assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task PutRestaurant_WithCorrectData_ReturnsSuccessResult()
    {
        // arrange
        var rest = Generator.GenerateRestaurant(Guid.NewGuid());
        _rests.Add(rest);

        var updateTo = Generator.GenerateRestaurant(Guid.NewGuid());
        var dto = updateTo.ToDto();

        _jwt.InitJwt("98423523", "Client");

        // act
        var result = await _sut.PutRestaurant(rest.Id, dto);

        // assert
        result.Should().BeOfType<OkObjectResult>();
        _rests.First().restaurantName.Should().Be(dto.restaurantName);
        _rests.First().imagePath.Should().Be(dto.imagePath);
        _rests.First().open_time.Should().Be(dto.open_time);
        _rests.First().close_time.Should().Be(dto.close_time);
        _rests.First().description.Should().Be(dto.description);
        _rests.First().phone_number.Should().Be(dto.phone_number);
        _rests.First().status.Should().Be(dto.status);
        _rests.First().user_id.Should().Be(rest.user_id);
        _rests.First().address.Should().Be(dto.address);
    }

    [Fact]
    public async Task PutRestaurant_WithWrongRestId_ReturnsNotFound()
    {
        // arrange
        var id = Guid.NewGuid(); 
        
        var updateTo = Generator.GenerateRestaurant(Guid.NewGuid());
        var dto = updateTo.ToDto();

        _jwt.InitJwt("98423523", "Client");

        // act
        var result = await _sut.PutRestaurant(id, dto);

        // assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Theory]
    [InlineData("Miruko", "")]
    [InlineData("", "Moscow")]
    public async Task PutRestaurant_WithWrongDtoData_ReturnsBadRequest(string restaurantName, string address)
    {
        // arrange
        var id = Guid.NewGuid();

        var updateTo = Generator.GenerateRestaurant(Guid.NewGuid());
        var dto = updateTo.ToDto();
        dto.restaurantName = restaurantName;
        dto.address = address;

        _jwt.InitJwt("98423523", "Client");

        // act
        var result = await _sut.PutRestaurant(id, dto);

        // assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task PutRestaurant_WithNullDto_ReturnsBadRequest()
    {
        // arrange
        var id = Guid.NewGuid();
        Restaurants_DTO dto = null;

        _jwt.InitJwt("98423523", "Client");

        // act
        var result = await _sut.PutRestaurant(id, dto);

        // assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }
}
