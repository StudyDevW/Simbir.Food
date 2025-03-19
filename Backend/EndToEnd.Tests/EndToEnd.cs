using ClientAPI.Controllers;
using Moq;
using PaymentAPI.Controllers;
using Telegram.Bot.Types;
using Telegram_Components.Interfaces;
using TestsBaseLib.Base;
using RabbitMQListenerServiceClient = ClientAPI.Services.RabbitMQListenerService;
using RabbitMQListenerServiceRestaurant = RestaurantAPI.Services.RabbitMQListenerService;
using RabbitMQListenerServicePayment = PaymentAPI.Services.RabbitMQListenerService;
using RabbitMQListenerServiceORM = ORM_Components.Services.RabbitMQListenerService;
using ORM_Components.DTO.PaymentAPI;
using ORM_Components.Tables;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using ORM_Components.DTO.ClientAPI;
using ORM_Components.Tables.Helpers;
using Microsoft.AspNetCore.Http;
using Middleware_Components.DTO.ClientAPI;
using CourierAPI.Controllers;
using System.Security.Claims;
using RestaurantAPI.Model.Controllers;
using ORM_Components.DTO.RestaurantAPI;

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
        var mailService = GetMailSender();
        var restaurantFoodItemsService = GetFoodService(context, jwtService);
        var restService = GetRestaurantService(context, tgService);

        // Init AuthController
        var authController = new AuthController(clientService, jwtService, cacheService, this.Configuration);
        
        var receiver = GetTelegramMessageReceiver(GetBotClient(), GetOperations(context), cacheService);

        // Init PaymentController
        var paymentController = new PaymentController(paymentService);

        // Init RequestRolesController
        var requestController = new RequestRolesController(clientService, jwtService, cacheService, Configuration);
        requestController.ControllerContext = new ControllerContext();
        requestController.ControllerContext.HttpContext = new DefaultHttpContext();

        // Init CourierController
        var accessor = new HttpContextAccessor();
        accessor.HttpContext = new DefaultHttpContext();
        Action<string> authCourierAction = (string user_id) => accessor.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim("Id", user_id)
        }));
        var courierService = GetCourierService(context, tgService, rabbitService, mailService, accessor);
        var courierController = new CourierController(courierService);

        // Init RabbitListeners
        var clientListener = new RabbitMQListenerServiceClient(databaseService, tgService, rabbitService);
        var paymentListener = new RabbitMQListenerServicePayment(paymentService, rabbitService);
        var restListener = new RabbitMQListenerServiceRestaurant(rabbitService, tgService, context);
        var ormListener = new RabbitMQListenerServiceORM(mailService, rabbitService);

        var cancel = new CancellationToken();
        await clientListener.StartAsync(cancel);
        await restListener.StartAsync(cancel);
        await paymentListener.StartAsync(cancel);
        await ormListener.StartAsync(cancel);

        // Init RestaurantFoodItemsController
        var foodsController = new RestaurantFoodItemsController(context, jwtService, restaurantFoodItemsService);

        // Init BasketController
        var basketController = new BasketController(clientService, jwtService, cacheService, Configuration);
        basketController.ControllerContext = new ControllerContext();
        basketController.ControllerContext.HttpContext = new DefaultHttpContext();

        // Init OrderController
        var orderController = new OrderController(clientService, jwtService, cacheService, Configuration);
        orderController.ControllerContext = new ControllerContext();
        orderController.ControllerContext.HttpContext = new DefaultHttpContext();

        // Init RestaurantController
        var restController = new RestaurantController(context, jwtService, restService);

        // Init ClientsController
        var clientsController = new ClientsController(clientService, jwtService, cacheService, Configuration);
        clientsController.ControllerContext = new ControllerContext();
        clientsController.ControllerContext.HttpContext = new DefaultHttpContext();

        // Clears Data
        ClearDatabase(context);
        ClearRedis();

        rabbitService.QueuePurge("payment_to_client");
        rabbitService.QueuePurge("payment_to_client_access_moneyback");
        rabbitService.QueuePurge("payment_to_client_error");
        rabbitService.QueuePurge("client_to_payment");
        rabbitService.QueuePurge("client_to_restaurant");

        // Init Test Data
        var card = new BankCardTable //todo: problem.1 - Adding bank card is not realized by programmers. Why do we need "cardUsersTable"?
        {
            card_number = "8888 1111 2222 3333",
            cvv = "444",
            money_value = 10000,
            name_card = "Visa"
        };
        context.bankCardTable.Add(card);

        var registerUserDto = Generator.GenerateUser().ToDto("PC");
        var restOwner = Generator.GenerateUser();
        var courier = Generator.GenerateUser();
        context.userTable.AddRange(restOwner, courier);

        var admin = Generator.GenerateUser("Admin");
        var adminDto = admin.ToDto("PC");
        context.userTable.Add(admin);

        context.SaveChanges();
        authCourierAction(courier.Id.ToString());        

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

        // user authorization
        var authUser = await authController.UserTelegramAuth(user.ToDto("Mobile"));
        var authUserTokens = (authUser as OkObjectResult).Value as Auth_PairTokens;
        setUser(basketController, authUserTokens.accessToken);
        setUser(clientsController, authUserTokens.accessToken);
        //

        // sets user's email for receiving checks
        var changeEmailAction = await clientsController.ChangeEmailForClient(new EmailAdd { email = Configuration["EMAILRECEIVER"]! });
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

        // restaurant owner authorization
        var authRestOwner = await authController.UserTelegramAuth(restOwner.ToDto("Mobile"));
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

        // admin authorization (for special functions like accepting requests)
        var authAdmin = await authController.UserTelegramAuth(adminDto);
        var authAdminTokens = (authAdmin as OkObjectResult).Value as Auth_PairTokens;
        setUser(requestController, authAdminTokens.accessToken);
        //

        // accepting restaurant request by admin
        var acceptRequestDto = new RequestAcceptReject { requestId = restRequest.Id };
        var acceptRestaurantRequestAction = await requestController.AcceptRestaurantRequest(acceptRequestDto);
        //

        // courier authorization
        var authCourier = await authController.UserTelegramAuth(courier.ToDto("Mobile"));
        var authCourierTokens = (authCourier as OkObjectResult).Value as Auth_PairTokens;
        setUser(requestController, authCourierTokens.accessToken);
        //

        // creation courier registration request
        var courierAddRequestDto = new CourierAddRequest
        {
            request_description = "please",
            car_number = null,
        };
        var createCourierRequestAction = await requestController.CreateCourierRequest(courierAddRequestDto);

        var courierTable = context.courierTable.First(x => x.userId == courier.Id);
        var courierRequest = context.requestTable.First(x => x.courier_id == courierTable.Id);
        //

        // again auth admin
        setUser(requestController, authAdminTokens.accessToken);
        //

        // accepting courier request by admin
        var acceptRequestCourierDto = new RequestAcceptReject { requestId = courierRequest.Id };
        var acceptCourierRequestAction = await requestController.AcceptCourierRequest(acceptRequestCourierDto);
        //

        // restaurant adds food items
        var foodItem1 = Generator.GenerateFoodItem(rest.Id, 1499).ToDto();
        var foodItem2 = Generator.GenerateFoodItem(rest.Id, 259).ToDto();
        var foodItem3 = Generator.GenerateFoodItem(rest.Id, 699).ToDto();
        
        var addFoodItemAction1 = await foodsController.AddRestaurantFoodItems(foodItem1);
        var addFoodItemAction2 = await foodsController.AddRestaurantFoodItems(foodItem2);
        var addFoodItemAction3 = await foodsController.AddRestaurantFoodItems(foodItem3);

        var food1 = context.restaurantFoodItemsTable.First(x => x.name == foodItem1.name);
        var food2 = context.restaurantFoodItemsTable.First(x => x.name == foodItem2.name);
        var food3 = context.restaurantFoodItemsTable.First(x => x.name == foodItem3.name);
        //

        // user adds food items to basket
        var addBasket1 = await basketController.AddItem(new Basket_Add { food_item_id = food1.Id, user_id = user.Id });
        var addBasket2 = await basketController.AddItem(new Basket_Add { food_item_id = food2.Id, user_id = user.Id });
        var addBasket3 = await basketController.AddItem(new Basket_Add { food_item_id = food3.Id, user_id = user.Id });
        //

        // user creates an order
        setUser(orderController, authUserTokens.accessToken);
        var createOrderAction = await orderController.CreateOrder();
        await Task.Delay(300); // waiting for rabbit listener actions (auto-accepts order)

        var order = context.orderTable.First();
        //

        // sets order status to WaitingForDelivery
        var setStatusOrderAction = await restController.SetReadyStatusForOrder(order.Id);
        //

        // courier gets list of order he can deliver
        var ordersToDeliver = await courierController.GetOrderList();
        var ourOrder = ordersToDeliver!.Value!.First();
        //

        // courier accepts an order
        var acceptOrderAction = await courierController.AcceptOrder(ourOrder.orderId);
        //

        // courier delivered and waiting for user
        var courierOnPlace = await courierController.CourierOnPlace(ourOrder.orderId);
        //

        // courier gives an order to user
        var deliveredOrderAction = await courierController.Delivered(ourOrder.orderId);
        await Task.Delay(2000); // waiting for rabbit listener actions (send email to user)
        //

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

        #endregion

        #region ASSERT

        // stopping rabbit listeners
        var cancelStop = new CancellationToken();
        await clientListener.StopAsync(cancelStop);
        await restListener.StopAsync(cancelStop);
        await paymentListener.StopAsync(cancelStop);
        await ormListener.StopAsync(cancelStop);

        registrationAction.Should().BeOfType<OkObjectResult>();
        payForUserAction.Should().BeOfType<OkObjectResult>();
        createRestaurantRequestAction.Should().BeOfType<OkObjectResult>();
        acceptRestaurantRequestAction.Should().BeOfType<OkObjectResult>();
        createCourierRequestAction.Should().BeOfType<OkObjectResult>();
        acceptCourierRequestAction.Should().BeOfType<OkObjectResult>();

        authAdmin.Should().BeOfType<OkObjectResult>();
        authUser.Should().BeOfType<OkObjectResult>();
        authCourier.Should().BeOfType<OkObjectResult>();
        authRestOwner.Should().BeOfType<OkObjectResult>();

        addFoodItemAction1.Should().BeOfType<OkObjectResult>();
        addFoodItemAction2.Should().BeOfType<OkObjectResult>();
        addFoodItemAction3.Should().BeOfType<OkObjectResult>();

        addBasket1.Should().BeOfType<OkResult>();
        addBasket2.Should().BeOfType<OkResult>();
        addBasket3.Should().BeOfType<OkResult>();

        createOrderAction.Should().BeOfType<OkObjectResult>();
        setStatusOrderAction.Should().BeOfType<OkResult>();

        acceptOrderAction.Should().BeOfType<NoContentResult>();
        courierOnPlace.Should().BeOfType<NoContentResult>();
        deliveredOrderAction.Should().BeOfType<NoContentResult>();

        changeEmailAction.Should().BeOfType<OkObjectResult>();

        #endregion
    }
}
