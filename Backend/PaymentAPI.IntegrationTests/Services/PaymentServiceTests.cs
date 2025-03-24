using FluentAssertions;
using Moq;
using ORM_Components.DTO.PaymentAPI;
using ORM_Components.Tables;
using ORM_Components.Tables.Helpers;
using PaymentAPI.Services;
using Telegram_Components.Interfaces;
using TestsBaseLib.Base;
using RabbitMQListenerServiceClient = ClientAPI.Services.RabbitMQListenerService;

namespace PaymentAPI.IntegrationTests.Services;

public class PaymentServiceTests : IntegrationTest
{
    /// <summary>
    /// Тестирует метод Pay класса PaymentService. Сложность теста заключается в одновременном тестировании
    /// Rabbit прослушки (ClientAPI). Перед вызовом метода Pay мы начинаем прослушку очереди payment_to_client.
    /// После вызова метода Pay стоит заглушка на 100мс, чтобы прослушка успела обработать запрос. По завершению
    /// ожидания 100мс, останавливаем прослушку и проверяем состояния.
    /// </summary>
    [Fact]
    public async Task Pay_WithCorrectData_WithdrawCardMoneyAndTopupUserBalance()
    {
        // arrange
        var rabbit = GetRabbitService();
        var context = GetDbContext();
        var sut = new PaymentService(rabbit, context);

        ClearDatabase(context);

        var database = GetDataService(context);
        var sender = Mock.Of<IMessageSender>();
        var listener = new RabbitMQListenerServiceClient(database, sender, rabbit);

        var user = Generator.GenerateUser();
        user.money_value = 200;

        var card = new BankCardTable
        {
            card_number = "8592 2157 2178 2589",
            cvv = "111",
            Id = Guid.NewGuid(),
            money_value = 500,
            name_card = "MasterCard"
        };

        var release = new Payment_Release
        {
            card_number = card.card_number,
            cvv = card.cvv,
            link_card = false,
            money_value = 400,
            user_id = user.Id
        };

        context.userTable.Add(user);
        context.bankCardTable.Add(card);
        context.SaveChanges();

        rabbit.QueuePurge("payment_to_client");
        await listener.StartAsync(new CancellationToken());

        // act
        await sut.Pay(release);
        await Task.Delay(300);

        // assert

        await listener.StopAsync(new CancellationToken());

        var pay = context.payTable.First();
        pay.card_number.Should().Be(card.card_number);
        pay.user_id.Should().Be(user.Id);
        pay.pay_status.Should().Be(PayStatus.Success);

        context.bankCardTable.First().money_value.Should().Be(100);

        context.userTable.First().money_value.Should().Be(600);
    }
}
