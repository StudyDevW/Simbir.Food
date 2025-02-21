using ClientAPI.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using Moq.EntityFrameworkCore;
using ORM_Components;
using ORM_Components.DTO.ClientAPI;
using ORM_Components.Tables;
using TestsBaseLib.Base;

namespace ClientAPI.Tests.Services;

/// <summary>
/// Unit tests of DatabaseService
/// </summary>
public class DatabaseServiceTests
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

    [Fact]
    public async Task InfoClientUpdate_WithNewPassword_UpdatesClientPassword()
    {
        // arrange
        var context = new Mock<DataContext>();
        var password = "pass123";
        var newPassword = "asd123";

        var user = new UserTable()
        {
            Id = Guid.NewGuid(),
            login = "tested"
        };

        var hasher = new PasswordHasher<PasswordAppUser>();
        var app = new PasswordAppUser { login = user.login };
        user.password = hasher.HashPassword(app, password);
        var users = new List<UserTable> { user };
        context.Setup(x => x.userTable).ReturnsDbSet(users);
        var dto = new ClientUpdate { password = newPassword };

        var sut = new DatabaseService(context.Object);

        // act
        await sut.InfoClientUpdate(dto, user.Id);

        // assert
        hasher.VerifyHashedPassword(app, user.password, newPassword)
            .Should().Be(PasswordVerificationResult.Success);
    }

    [Fact]
    public async Task InfoClientUpdate_WithTheSamePassword_ThrowsException()
    {
        // arrange
        var context = new Mock<DataContext>();
        var password = "pass123";
        var newPassword = "pass123";

        var user = new UserTable()
        {
            Id = Guid.NewGuid(),
            login = "tested"
        };

        var hasher = new PasswordHasher<PasswordAppUser>();
        var app = new PasswordAppUser { login = user.login };
        user.password = hasher.HashPassword(app, password);
        var users = new List<UserTable> { user };
        context.Setup(x => x.userTable).ReturnsDbSet(users);
        var dto = new ClientUpdate { password = newPassword };

        var sut = new DatabaseService(context.Object);

        // act
        Func<Task> act = async() => await sut.InfoClientUpdate(dto, user.Id);

        // assert
        await act.Should().ThrowAsync<Exception>().WithMessage("password_1:1");
    }

    [Fact]
    public async Task InfoClientUpdate_WithAllDataButPassword_UpdatesClientData()
    {
        // arrange
        var context = new Mock<DataContext>();

        var newName = "logan";
        var newAddress = "London";
        var newAvatarImage = "kinda_url";
        var newEmail = "logan@gmail.com";
        var newPhone = "892018626";

        var user = new UserTable()
        {
            Id = Guid.NewGuid(),
            address = "Moscow",
            avatarImage = "some_url",
            email = "moscow@gmail.com",
            name = "potap",
            phone_number = "790258912"
        };

        var users = new List<UserTable> { user };

        context.Setup(x => x.userTable).ReturnsDbSet(users);
        var dto = new ClientUpdate
        {
            name = newName,
            address = newAddress,
            avatarImage = newAvatarImage,
            email = newEmail,
            phone_number = newPhone,
        };

        var sut = new DatabaseService(context.Object);

        // act
        await sut.InfoClientUpdate(dto, user.Id);

        // assert
        user.name.Should().Be(newName);
        user.address.Should().Be(newAddress);
        user.avatarImage.Should().Be(newAvatarImage);
        user.email.Should().Be(newEmail);
        user.phone_number.Should().Be(newPhone);
    }

    [Fact]
    public async Task InfoClientUpdate_WithNullData_DoesNothing()
    {
        // arrange
        var context = new Mock<DataContext>();

        var address = "Moscow";
        var avatarImage = "some_url";
        var email = "moscow@gmail.com";
        var name = "potap";
        var phone_number = "790258912";

        var user = new UserTable()
        {
            Id = Guid.NewGuid(),
            address = address,
            avatarImage = avatarImage,
            email = email,
            name = name,
            phone_number = phone_number
        };

        var users = new List<UserTable> { user };

        context.Setup(x => x.userTable).ReturnsDbSet(users);
        var dto = new ClientUpdate();

        var sut = new DatabaseService(context.Object);

        // act
        await sut.InfoClientUpdate(dto, user.Id);

        // assert
        user.name.Should().Be(name);
        user.address.Should().Be(address);
        user.avatarImage.Should().Be(avatarImage);
        user.email.Should().Be(email);
        user.phone_number.Should().Be(phone_number);
    }

    [Fact]
    public async Task InfoClientUpdate_WithNullUser_ThrowsException()
    {
        // arrange
        var context = new Mock<DataContext>();
        var users = new List<UserTable>();

        context.Setup(x => x.userTable).ReturnsDbSet(users);
        
        var dto = new ClientUpdate();
        var sut = new DatabaseService(context.Object);

        // act
        Func<Task> act = async () => await sut.InfoClientUpdate(dto, Guid.NewGuid());

        // assert
        await act.Should().ThrowAsync<Exception>().WithMessage("user_not_found");
    }

    [Fact]
    public async Task DeleteClientWithAdmin_WithUserId_DeletesUser()
    {
        // arrange
        var context = new Mock<DataContext>();

        var user = new UserTable
        {
            Id = Guid.NewGuid(),
        };

        var users = new List<UserTable> { user };

        var restaurant = new RestaurantTable
        {
            Id = Guid.NewGuid(),
            user_id = user.Id,
        };

        var restaurants = new List<RestaurantTable> { restaurant };

        var items = new List<RestaurantFoodItemsTable> 
        {
            new RestaurantFoodItemsTable
            {
                Id = Guid.NewGuid(),
                restaurant_id = restaurant.Id,
            },
            new RestaurantFoodItemsTable
            {
                Id = Guid.NewGuid(),
                restaurant_id = restaurant.Id,
            },
            new RestaurantFoodItemsTable
            {
                Id = Guid.NewGuid(),
                restaurant_id = restaurant.Id,
            }
        };

        var couriers = new List<CourierTable> 
        {
            new CourierTable
            {
                Id = Guid.NewGuid(),
                userId = user.Id
            }
        };

        context.Setup(x => x.userTable).ReturnsDbSet(users);
        context.Setup(x => x.restaurantTable).ReturnsDbSet(restaurants);
        context.Setup(x => x.restaurantFoodItemsTable).ReturnsDbSet(items);
        context.Setup(x => x.courierTable).ReturnsDbSet(couriers);

        context.Setup(x => x.userTable.Remove(It.IsAny<UserTable>()))
            .Callback<UserTable>(x => users.Remove(x));
        context.Setup(x => x.restaurantTable.Remove(It.IsAny<RestaurantTable>()))
            .Callback<RestaurantTable>(x => restaurants.Remove(x));
        context.Setup(x => x.restaurantFoodItemsTable.Remove(It.IsAny<RestaurantFoodItemsTable>()))
            .Callback<RestaurantFoodItemsTable>(x => items.Remove(x));
        context.Setup(x => x.courierTable.Remove(It.IsAny<CourierTable>()))
            .Callback<CourierTable>(x => couriers.Remove(x));

        var sut = new DatabaseService(context.Object);

        // act
        await sut.DeleteClientWithAdmin(user.Id);

        // assert
        users.Count.Should().Be(0);
        restaurants.Count.Should().Be(0);
        items.Count.Should().Be(0);
        couriers.Count.Should().Be(0);
    }

    [Fact]
    public async Task DeleteClientWithAdmin_WithNotExistingUser_ThrowsException()
    {
        // arrange
        var context = new Mock<DataContext>();

        var users = new List<UserTable>();
        context.Setup(x => x.userTable).ReturnsDbSet(users);

        var sut = new DatabaseService(context.Object);

        // act
        Func<Task> act = async() => await sut.DeleteClientWithAdmin(Guid.NewGuid());

        // assert
        await act.Should().ThrowAsync<Exception>().WithMessage("user_not_found");
    }
}
