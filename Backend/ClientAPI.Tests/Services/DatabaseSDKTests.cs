using ClientAPI.Database;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.EntityFrameworkCore;
using ORM_Components;
using ORM_Components.DTO.ClientAPI;
using ORM_Components.Tables;
using TestsBaseLib.Base;

namespace ClientAPI.Tests.Services;

public class DatabaseSDKTests : BaseTest
{
    [Fact]
    public async Task RegisterUser_WithCorrentData_AddedUserInDb()
    {
        // arrange
        var configuration = Mock.Of<IConfiguration>();
        var context = await GetDbContext();
        var dto = new AuthSignUp
        {
            address = "Addr",
            email = "test@gmail.com",
            login = "tested",
            name = "Dominik",
            password = "test123",
            phone_number = "79278567486"
        };
        var sut = new DatabaseSDK(configuration, context);

        // act
        await sut.RegisterUser(dto);

        // assert
        var user = await context.userTable.FirstOrDefaultAsync();
        user.Should().NotBeNull();
        user.email.Should().Be("test@gmail.com");
        user.password.Should().NotBe(dto.password);
    }

    [Fact]
    public async Task RegisterUser_EmptyDto_CallsError()
    {
        // arrange
        var configuration = Mock.Of<IConfiguration>();
        var context = await GetDbContext();
        var dto = new AuthSignUp();
        var sut = new DatabaseSDK(configuration, context);

        // act
        Func<Task> act = async () => await sut.RegisterUser(dto);

        // assert
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task RegisterUser_NullDto_UserTableStayEmpty()
    {
        // arrange
        var configuration = Mock.Of<IConfiguration>();
        var context = await GetDbContext();
        AuthSignUp dto = null;
        var sut = new DatabaseSDK(configuration, context);

        // act
        await sut.RegisterUser(dto);

        // assert
        var user = await context.userTable.CountAsync();
        user.Should().Be(0);
    }

    [Fact]
    public void CheckUser_WithCorrectData_ReturnSuccess()
    {
        // arrange
        var configuration = Mock.Of<IConfiguration>();
        var contextStub = new Mock<DataContext>();

        var login = "login123";
        var password = "pass123";

        var hasher = new PasswordHasher<PasswordAppUser>();
        var passwordUser = new PasswordAppUser() { login = login };

        var list = new List<UserTable> {
            new UserTable
            {
                login = login,
                password = hasher.HashPassword(passwordUser, password),
                roles = new string[] { "Client" }
            }
        };

        contextStub.Setup(x => x.userTable).ReturnsDbSet(list);

        var sut = new DatabaseSDK(configuration, contextStub.Object);

        var dto = new AuthSignIn
        {
            login = login, 
            password = password
        };

        // act
        var result = sut.CheckUser(dto);

        // assert
        result.check_success.Should().NotBeNull();
        result.check_success.username.Should().Be(login);
        result.check_error.Should().BeNull();
    }

    [Fact]
    public void CheckUser_NullDto_ReturnError()
    {
        // arrange
        var configuration = Mock.Of<IConfiguration>();
        var contextStub = new Mock<DataContext>();
        var sut = new DatabaseSDK(configuration, contextStub.Object);
        AuthSignIn dto = null;

        // act
        var result = sut.CheckUser(dto);

        // assert
        result.check_error.Should().NotBeNull();
        result.check_success.Should().BeNull();
    }

    [Fact]
    public void CheckUser_UserDoesntExist_ReturnError()
    {
        // arrange
        var configuration = Mock.Of<IConfiguration>();
        var contextStub = new Mock<DataContext>();
        var sut = new DatabaseSDK(configuration, contextStub.Object);
        var dto = new AuthSignIn
        {
            login = "login123",
            password = "pass123"
        };
        contextStub.Setup(x => x.userTable).ReturnsDbSet(new List<UserTable>());

        // act
        var result = sut.CheckUser(dto);

        // assert
        result.check_error.Should().NotBeNull();
        result.check_success.Should().BeNull();
    }

    [Fact]
    public void CheckUser_WithWrongPassword_ReturnError()
    {
        // arrange
        var configuration = Mock.Of<IConfiguration>();
        var contextStub = new Mock<DataContext>();

        var login = "login123";
        var password = "pass123";

        var hasher = new PasswordHasher<PasswordAppUser>();
        var passwordUser = new PasswordAppUser() { login = login };

        var list = new List<UserTable> {
            new UserTable
            {
                login = login,
                password = hasher.HashPassword(passwordUser, password),
                roles = new string[] { "Client" }
            }
        };

        contextStub.Setup(x => x.userTable).ReturnsDbSet(list);

        var sut = new DatabaseSDK(configuration, contextStub.Object);

        var dto = new AuthSignIn
        {
            login = login,
            password = "wrongpassword"
        };

        // act
        var result = sut.CheckUser(dto);

        // assert
        result.check_success.Should().BeNull();
        result.check_error.Should().NotBeNull();
    }
}
