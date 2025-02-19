using ClientAPI.Services;
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

public class DatabaseServiceTests : BaseTest
{
    [Fact]
    public async Task RegisterUser_WithCorrentData_AddedUserInDb()
    {
        // arrange
        var context = new Mock<DataContext>();
        var users = new List<UserTable>();

        context.Setup(x => x.userTable).ReturnsDbSet(users);
        context.Setup(x => x.userTable.Add(It.IsAny<UserTable>()))
            .Callback<UserTable>(x => users.Add(x));

        var dto = new AuthSignUp
        {
            address = "Addr",
            email = "test@gmail.com",
            login = "tested",
            name = "Dominik",
            password = "test123",
            phone_number = "79278567486"
        };

        var sut = new DatabaseService(context.Object);

        // act
        await sut.RegisterUser(dto);

        // assert
        var user = users.FirstOrDefault();
        user.Should().NotBeNull();
        user.email.Should().Be("test@gmail.com");
        user.password.Should().NotBe(dto.password);
    }

    [Fact]
    public async Task RegisterUser_WithExistingUser_ThrowsException()
    {
        // arrange
        var context = new Mock<DataContext>();

        var login = "tested";
        var users = new List<UserTable>
        {
            new UserTable { login = login }
        };

        context.Setup(x => x.userTable).ReturnsDbSet(users);
        context.Setup(x => x.userTable.Add(It.IsAny<UserTable>()))
            .Callback<UserTable>(x => users.Add(x));

        var dto = new AuthSignUp
        {
            address = "Addr",
            email = "test@gmail.com",
            login = login,
            name = "Dominik",
            password = "test123",
            phone_number = "79278567486"
        };

        var sut = new DatabaseService(context.Object);

        // act
        Func<Task> act = async () => await sut.RegisterUser(dto);

        // assert
        await act.Should().ThrowAsync<Exception>().WithMessage("login already exist");
    }

    [Fact]
    public async Task RegisterUser_NullDto_ThrowsException()
    {
        // arrange
        var context = Mock.Of<DataContext>();
        AuthSignUp dto = null;
        var sut = new DatabaseService(context);

        // act
        Func<Task> act = async () => await sut.RegisterUser(dto);

        // assert
        await act.Should().ThrowAsync<Exception>().WithMessage("dto null");
    }

    [Fact]
    public void CheckUser_WithCorrectData_ReturnSuccess()
    {
        // arrange
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

        var sut = new DatabaseService(contextStub.Object);

        var dto = new AuthSignIn
        {
            login = login,
            password = password
        };

        // act
        var result = sut.CheckUser(dto);

        // assert
        result.check_success.Should().NotBeNull();
        result.check_success.login.Should().Be(login);
        result.check_error.Should().BeNull();
    }

    [Fact]
    public void CheckUser_NullDto_ReturnError()
    {
        // arrange
        var contextStub = new Mock<DataContext>();
        var sut = new DatabaseService(contextStub.Object);
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
        var contextStub = new Mock<DataContext>();
        var sut = new DatabaseService(contextStub.Object);
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

        var sut = new DatabaseService(contextStub.Object);

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

    private List<UserTable> GetTestUsers() => new List<UserTable>
    {
        new UserTable { login = "user_1", roles = new string[] { "Client" } },
        new UserTable { login = "user_2", roles = new string[] { "Client" } },
        new UserTable { login = "user_3", roles = new string[] { "Client" } },
        new UserTable { login = "user_4", roles = new string[] { "Client" } },
        new UserTable { login = "user_5", roles = new string[] { "Client" } },
        new UserTable { login = "user_6", roles = new string[] { "Client" } },
        new UserTable { login = "user_7", roles = new string[] { "Client" } },
        new UserTable { login = "user_8", roles = new string[] { "Client" } },
        new UserTable { login = "user_9", roles = new string[] { "Client" } },
        new UserTable { login = "user_10", roles = new string[] { "Client" } },
        new UserTable { login = "user_11", roles = new string[] { "Client" } },
        new UserTable { login = "user_12", roles = new string[] { "Client" } },
        new UserTable { login = "user_13", roles = new string[] { "Client" } },
        new UserTable { login = "user_14", roles = new string[] { "Client" } },
        new UserTable { login = "user_15", roles = new string[] { "Client" } },
    };

    [Theory]
    [InlineData(2, 6, "user_3", "user_8")]
    [InlineData(0, 4, "user_1", "user_4")]
    [InlineData(5, 1, "user_6", "user_6")]
    [InlineData(13, 2, "user_14", "user_15")]
    public void GetAllClients_WithCountBiggerThanZero_ReturnsClientGetAll(int from, int count, string firstName, string lastName)
    {
        // arrange
        var context = new Mock<DataContext>();
        var users = GetTestUsers();

        context.Setup(x => x.userTable).ReturnsDbSet(users);

        var sut = new DatabaseService(context.Object);

        // act
        var result = sut.GetAllClients(from, count);

        // assert
        result.Content.First().login.Should().Be(firstName);
        result.Content.Count.Should().Be(count);
        result.Content.Last().login.Should().Be(lastName);
    }

    [Theory]
    [InlineData(2, "user_3", "user_15")]
    [InlineData(0, "user_1", "user_15")]
    [InlineData(14, "user_15", "user_15")]
    public void GetAllClients_WithCountEqualsZero_ReturnsClientGetAll(int from, string firstName, string lastName)
    {
        // arrange
        var context = new Mock<DataContext>();
        var users = GetTestUsers();

        context.Setup(x => x.userTable).ReturnsDbSet(users);

        var sut = new DatabaseService(context.Object);

        // act
        var result = sut.GetAllClients(from, 0);

        // assert
        result.Content.First().login.Should().Be(firstName);
        result.Content.Count.Should().Be(users.Count - from);
        result.Content.Last().login.Should().Be(lastName);
    }
}
