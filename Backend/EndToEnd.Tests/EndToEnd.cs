using ClientAPI.Controllers;
using Moq;
using PaymentAPI.Controllers;
using Telegram.Bot.Types;
using Telegram_Components.Interfaces;
using TestsBaseLib.Base;
using RabbitMQListenerServiceClient = ClientAPI.Services.RabbitMQListenerService;
using RabbitMQListenerServiceRestaurant = RestaurantAPI.Services.RabbitMQListenerService;
using RabbitMQListenerServicePayment = PaymentAPI.Services.RabbitMQListenerService;
using ORM_Components.DTO.PaymentAPI;
using ORM_Components.Tables;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using ORM_Components.DTO.ClientAPI;
using ORM_Components.Tables.Helpers;
using Microsoft.AspNetCore.Http;
using Middleware_Components.JWT.DTO.CheckUsers;
using Middleware_Components.DTO.ClientAPI;

namespace EndToEnd.Tests;

public class EndToEnd : IntegrationTest
{
    private void setUser(ControllerBase controller, string accessToken)
    {
        controller.ControllerContext.HttpContext.Request.Headers.Authorization = "Bearer " + accessToken;
    }

    [Fact]
    public async Task TestFromUserRegistrationAndOrderCreationToOrderMakingByRestaurantAndDeliveringByCourier()
    {
        #region ARRANGE

        // Init Services
        var rabbitService = GetRabbitService();
        var tgService = Mock.Of<IMessageSender>();
        var cacheService = GetCacheService(GetConnectionMultiplexer());
        var sessionService = GetSessionService(cacheService);
        var context = GetDbContext();
        var databaseService = GetDataService(context);
        var jwtService = GetJwtService(cacheService);
        var clientService = GetClientService(rabbitService, tgService, sessionService, databaseService, jwtService, cacheService);
        var paymentService = GetPaymentService(rabbitService, context);

        // Init AuthController
        var authController = new AuthController(clientService, jwtService, cacheService, this.Configuration);
        
        var registerUserDto = Generator.GenerateUser().ToDto("PC");
        var restOwnerRegisterDto = Generator.GenerateUser().ToDto("Mobile");
        var courierRegisterDto = Generator.GenerateUser().ToDto("Mobile");

        var receiver = GetTelegramMessageReceiver(GetBotClient(), GetOperations(context), cacheService);

        // Init PaymentController
        var paymentController = new PaymentController(paymentService);

        // Init RequestRolesController
        var requestController = new RequestRolesController(clientService, jwtService, cacheService, Configuration);
        requestController.ControllerContext = new ControllerContext();
        requestController.ControllerContext.HttpContext = new DefaultHttpContext();

        // Init RabbitListeners
        var clientListener = new RabbitMQListenerServiceClient(databaseService, tgService, rabbitService);
        var paymentListener = new RabbitMQListenerServicePayment(paymentService, rabbitService);
        var restListener = new RabbitMQListenerServiceRestaurant(rabbitService, tgService, context);

        var cancel = new CancellationToken();
        await clientListener.StartAsync(cancel);
        await restListener.StartAsync(cancel);
        await paymentListener.StartAsync(cancel);

        // Clears Data
        ClearDatabase(context);
        ClearRedis();

        rabbitService.QueuePurge("payment_to_client");
        rabbitService.QueuePurge("payment_to_client_access_moneyback");
        rabbitService.QueuePurge("payment_to_client_error");
        rabbitService.QueuePurge("client_to_payment");
        rabbitService.QueuePurge("client_to_restaurant");

        #endregion

        #region ACT

        // creates registration request
        var registrationAction = await authController.UserTelegramRegister(registerUserDto);
        //

        // accepting a request by user
        await receiver.handleCallbackQuery(new CallbackQuery
        {
            From = new User { Id = registerUserDto.id },
            Message = new Message { Id = 92525252 },
            Id = "89252532",
            Data = "registerQuery"
        });
        var user = context.userTable.First(x => x.telegram_id == registerUserDto.id);
        //

        // connecting card to profile
        var card = new BankCardTable //todo: problem.1 - Adding bank card is not realized by programmers. Why do we need "cardUsersTable"?
        {
            card_number = "8888 1111 2222 3333",
            cvv = "444",
            money_value = 10000,
            name_card = "Visa"
        };
        context.bankCardTable.Add(card);
        context.SaveChanges();
        //

        // toping up user's balance using his card
        var paymentDto = new Payment_Release // todo: problem.2 - how to understand that it's my card? (we have not user_id in card, so it means that no one has this card)  
        {
            card_number = card.card_number,
            cvv = card.cvv,
            link_card = false,
            money_value = 8000,
            user_id = user.Id
        };
        var payForUserAction = await paymentController.PayForUserBalance(paymentDto);
        await Task.Delay(300); // waiting for rabbit listener actions (increase user's balance)
        //

        // restaurant owner registration
        var restOwnerRegistrationAction = await authController.UserTelegramRegister(restOwnerRegisterDto);
        //

        // accepting a restraurant owner request by owner
        await receiver.handleCallbackQuery(new CallbackQuery
        {
            From = new User { Id = restOwnerRegisterDto.id },
            Message = new Message { Id = 92525252 },
            Id = "89252532",
            Data = "registerQuery"
        });
        var restOwner = context.userTable.First(x => x.telegram_id == restOwnerRegisterDto.id);
        //

        // restaurant owner authorization
        var authRestOwner = await authController.UserTelegramAuth(restOwnerRegisterDto);
        var authRestOwnerTokens = (authRestOwner as OkObjectResult).Value as Auth_PairTokens;
        setUser(requestController, authRestOwnerTokens.accessToken);
        //

        // creation restaurant registration request
        var restaurantAddRequestDto = Generator.GenerateRestaurant(restOwner.Id, RestaurantStatus.Unverified)
            .ToRequestDto("a small restaurant in the village");

        var createRestaurantRequestAction = await requestController.CreateRestaurantRequest(restaurantAddRequestDto);
        var rest = context.restaurantTable.First(x => x.user_id == restOwner.Id);
        var restRequest = context.requestTable.First(x => x.restaurant_id == rest.Id);
        //

        // creates admin
        var admin = Generator.GenerateUser("Admin");
        context.userTable.Add(admin);
        context.SaveChanges();
        var adminDto = admin.ToDto("PC");
        //

        // admin authorization (for special functions like accepting requests)
        var authAdmin = await authController.UserTelegramAuth(adminDto);
        var authAdminTokens = (authAdmin as OkObjectResult).Value as Auth_PairTokens;
        setUser(requestController, authAdminTokens.accessToken);
        //

        // accepting restaurant request by admin
        var acceptRequestDto = new RequestAcceptReject { requestId = restRequest.Id };
        var acceptRestaurantRequestAction = await requestController.AcceptRestaurantRequest(acceptRequestDto);
        //

        throw new Exception("В процессе написания");

        // 1. registration
        // 2. connect card to profile (not realized by programmers)
        // 3. user top up balance
        // *. restaurant registration and admin accepting
        // *. courier registration and admin accepting
        // 4. restaraunt adds food items
        // 5. user adds food items to basket
        // 6. user creates an order
        // 7. user pays for order
        // 8. restaurant accepting an order than making
        // 9. after finished sets status to waitingfordelivery
        // 10. courier accepting order
        // 11. courier delivered order
        // 12. user makes a review to order

        #endregion

        #region ASSERT

        // stopping rabbit listeners
        var cancelStop = new CancellationToken();
        await clientListener.StopAsync(cancelStop);
        await restListener.StopAsync(cancelStop);
        await paymentListener.StopAsync(cancelStop);

        registrationAction.Should().BeOfType<OkObjectResult>();
        payForUserAction.Should().BeOfType<OkObjectResult>();
        restOwnerRegistrationAction.Should().BeOfType<OkObjectResult>();
        createRestaurantRequestAction.Should().BeOfType<OkObjectResult>();
        acceptRestaurantRequestAction.Should().BeOfType<OkObjectResult>();

        #endregion
    }
}
