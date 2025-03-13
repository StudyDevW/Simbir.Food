using FluentAssertions;
using Middleware_Components.Broker;
using Moq;
using ORM_Components;
using ORM_Components.DTO.PaymentAPI;
using ORM_Components.Tables;
using ORM_Components.Tables.Helpers;
using PaymentAPI.Services;
using TestsBaseLib.Base;

namespace PaymentAPI.Tests.Services;

public class PaymentServiceTests : UnitTest
{
    private readonly PaymentService _sut;
    private readonly Mock<IRabbitMQService> _rabbit;

    public PaymentServiceTests()
    {
        _rabbit = new Mock<IRabbitMQService>();

        _sut = new PaymentService(_rabbit.Object, _context.Object);
    }

    [Fact]
    public async Task MoneyBack_WithCorrectData_CreatesPaymentOutAndSendsRabbitMessage()
    {
        // arrange
        var user = Generator.GenerateUser();
        var card = new BankCardTable
        {
            card_number = "2200 2551 8963 0362",
            cvv = "767",
            Id = Guid.NewGuid(),
            money_value = 400,
            name_card = "Visa"
        };

        itemsSetup(x => x.bankCardTable).AddItem(card);
        itemsSetup(x => x.userTable).AddItem(user);
        var pays = itemsSetup(x => x.payTable,
            add: x => x.payTable.Add(any<PayTable>()));

        var dto = new PaymentOut
        {
            card_number = card.card_number,
            money_value = 200,
            user_id = user.Id
        };

        // act
        await _sut.MoneyBack(dto);

        // assert
        var pay = pays.First();
        pay.card_number.Should().Be(card.card_number);
        pay.pay_status.Should().Be(PayStatus.MoneyBack);
        pay.user_id.Should().Be(user.Id);

        _rabbit.Verify(x => x.SendMessage("payment_to_client_access_moneyback", any<Payment_Success_Queue>()));
    }

    [Fact]
    public async Task MoneyBack_WithNonExistentCard_DoesNothingWithBalanceAndSendsRabbitMessage()
    {
        // arrange
        var user = Generator.GenerateUser();
        user.money_value = 400;

        itemsSetup(x => x.bankCardTable);
        itemsSetup(x => x.userTable).AddItem(user);
        var pays = itemsSetup(x => x.payTable,
            add: x => x.payTable.Add(any<PayTable>()));

        var dto = new PaymentOut
        {
            card_number = "2200 2551 8963 0362",
            money_value = 200,
            user_id = user.Id
        };

        // act
        await _sut.MoneyBack(dto);

        // assert
        pays.Count.Should().Be(0);
        user.money_value.Should().Be(400);
        _rabbit.Verify(x => x.SendMessage("payment_to_client_error", any<Payment_Error_Queue>()));
    }

    [Fact]
    public async Task Pay_WithCorrectData_WithdrawCardMoneyAndTopupUserBalance()
    {
        // arrange
        var user = Generator.GenerateUser();
        user.money_value = 100;

        var card = new BankCardTable
        {
            card_number = "2200 2551 8963 0362",
            cvv = "767",
            Id = Guid.NewGuid(),
            money_value = 400,
            name_card = "Visa"
        };

        itemsSetup(x => x.bankCardTable).AddItem(card);
        itemsSetup(x => x.userTable).AddItem(user);
        var pays = itemsSetup(x => x.payTable,
            add: x => x.payTable.Add(any<PayTable>()));

        var dto = new Payment_Release
        {
            card_number = card.card_number,
            cvv = card.cvv,
            link_card = false,
            money_value = 400,
            user_id = user.Id
        };

        // act
        await _sut.Pay(dto);

        // assert
        var pay = pays.First();
        pay.pay_status.Should().Be(PayStatus.Success);
        pay.card_number.Should().Be(card.card_number);
        pay.user_id.Should().Be(user.Id);

        card.money_value.Should().Be(0);
        user.money_value.Should().Be(100);

        _rabbit.Verify(x => x.SendMessage("payment_to_client", any<Payment_Success_Queue>()));
    }

    [Fact]
    public async Task Pay_WithNonExistentCard_ThrowsException()
    {
        // arrange
        var user = Generator.GenerateUser();
        user.money_value = 100;

        itemsSetup(x => x.bankCardTable);
        itemsSetup(x => x.userTable).AddItem(user);
        var pays = itemsSetup(x => x.payTable,
            add: x => x.payTable.Add(any<PayTable>()));

        var dto = new Payment_Release
        {
            card_number = "2200 2551 8963 0362",
            cvv = "567",
            link_card = false,
            money_value = 400,
            user_id = user.Id
        };

        // act
        Func<Task> act = async() => await _sut.Pay(dto);

        // assert
        await act.Should().ThrowAsync<Exception>().WithMessage("card_not_exist");
    }

    [Fact]
    public async Task Pay_WithWrongCVV_ThrowsException()
    {
        // arrange
        var user = Generator.GenerateUser();
        user.money_value = 100;

        var card = new BankCardTable
        {
            card_number = "2200 2551 8963 0362",
            cvv = "767",
            Id = Guid.NewGuid(),
            money_value = 400,
            name_card = "Visa"
        };

        itemsSetup(x => x.bankCardTable).AddItem(card);
        itemsSetup(x => x.userTable).AddItem(user);
        var pays = itemsSetup(x => x.payTable,
            add: x => x.payTable.Add(any<PayTable>()));

        var dto = new Payment_Release
        {
            card_number = card.card_number,
            cvv = "766",
            link_card = false,
            money_value = 400,
            user_id = user.Id
        };

        // act
        Func<Task> act = async () => await _sut.Pay(dto);

        // assert
        await act.Should().ThrowAsync<Exception>().WithMessage("card_not_verified");
    }

    [Fact]
    public async Task Pay_WithNotEnoughMoney_ThrowsException()
    {
        // arrange
        var user = Generator.GenerateUser();
        user.money_value = 100;

        var card = new BankCardTable
        {
            card_number = "2200 2551 8963 0362",
            cvv = "767",
            Id = Guid.NewGuid(),
            money_value = 400,
            name_card = "Visa"
        };

        itemsSetup(x => x.bankCardTable).AddItem(card);
        itemsSetup(x => x.userTable).AddItem(user);
        var pays = itemsSetup(x => x.payTable,
            add: x => x.payTable.Add(any<PayTable>()));

        var dto = new Payment_Release
        {
            card_number = card.card_number,
            cvv = card.cvv,
            link_card = false,
            money_value = 500,
            user_id = user.Id
        };

        // act
        Func<Task> act = async () => await _sut.Pay(dto);

        // assert
        await act.Should().ThrowAsync<Exception>().WithMessage("low_money_on_card");
    }
}
