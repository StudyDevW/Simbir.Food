using FluentAssertions;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using ORM_Components.DTO.ClientAPI;
using ORM_Components.Tables;
using Telegram_Components.Services;
using TestsBaseLib.Base;

namespace Telegram_Components.Tests.Services;

public class DatabaseOperationsTests : UnitTest
{
    private readonly DatabaseOperations _sut;
    private readonly List<UserTable> _users;

    public DatabaseOperationsTests()
    {
        _sut = new DatabaseOperations(_context.Object);

        _users = itemsSetup(x => x.userTable,
            add: x => x.userTable.Add(any<UserTable>()));
    }

    [Fact]
    public async Task AddUserFromTelegram_WithCorrectData_AddsUserToDb()
    {
        // arrange
        var user = Generator.GenerateUser();
        var dto = user.ToDto("PC");

        // act
        await _sut.AddUserFromTelegram(dto, false);

        // assert
        var newUser = _users.First();
        newUser.Should().BeEquivalentTo(user,
            x => x.Excluding(z => z.Id)
            .Excluding(z => z.email)
            );
    }

    [Fact]
    public async Task AddUserFromTelegram_WithNullDto_ThrowsException()
    {
        // arrange
        AuthAddUser dto = null!;

        // act
        Func<Task> act = async () => await _sut.AddUserFromTelegram(dto, false);

        // assert
        await act.Should().ThrowAsync<Exception>().WithMessage("dto null");
    }

    [Fact]
    public async Task AddUserFromTelegram_WithAlreadyExistentUserWithThatTelegramId_ThrowsException()
    {
        // arrange
        var existent = Generator.GenerateUser();
        _users.Add(existent);

        var user = Generator.GenerateUser();
        user.telegram_id = existent.telegram_id;
        var dto = user.ToDto("PC");

        // act
        Func<Task> act = async () => await _sut.AddUserFromTelegram(dto, false);

        // assert
        await act.Should().ThrowAsync<Exception>().WithMessage("user already exist");
    }
}
