using ClientAPI.Services;
using FluentAssertions;
using Moq;
using ORM_Components.DTO.ClientAPI;
using ORM_Components.Tables;
using Moq.EntityFrameworkCore;
using TestsBaseLib.Base;
using ORM_Components.Tables.Helpers;

namespace ClientAPI.Tests.Services;

public class DatabaseServiceTests : UnitTest
{
    private readonly DatabaseService _sut;

    private readonly List<UserTable> _users;

    public DatabaseServiceTests()
    {
        _users = new List<UserTable>();

        _users = itemsSetup(
            x => x.userTable,
            x => x.userTable.Remove(It.IsAny<UserTable>()));

        _sut = new DatabaseService(_context.Object);
    }

    [Fact]
    public void CheckUser_WithCorrectData_ReturnsSuccess()
    {
        // arrange
        var user = Generator.GenerateUser();
        _users.Add(user);

        var dto = new AuthSignIn
        {
            telegram_chat_id = user.telegram_chat_id,
            device = "windows 10 desktop"
        };

        // act
        var result = _sut.CheckUser(dto);

        // assert
        result.check_success.Should().NotBeNull();
        result.check_success.Id.Should().Be(user.Id);
        result.check_error.Should().BeNull();
    }

    [Fact]
    public void CheckUser_NullDto_ReturnsError()
    {
        // arrange
        AuthSignIn dto = null;

        // act
        var result = _sut.CheckUser(dto);

        // assert
        result.check_error.Should().NotBeNull();
        result.check_success.Should().BeNull();
    }

    [Fact]
    public void CheckUser_WithNonExistentUser_ReturnsError()
    {
        // arrange
        var dto = new AuthSignIn
        {
            telegram_chat_id = 58932532,
            device = "windows 10 desktop"
        };

        // act
        var result = _sut.CheckUser(dto);

        // assert
        result.check_success.Should().BeNull();
        result.check_error.Should().NotBeNull();
    }

    private List<UserTable> _GetTestUsers()
    {
        var list = new List<UserTable>();

        for (int i = 1; i <= 15; i++)
            list.Add(Generator.GenerateUserWithName($"user_{i}"));

        return list;
    }

    [Theory]
    [InlineData(2, 6, "user_3", "user_8")]
    [InlineData(0, 4, "user_1", "user_4")]
    [InlineData(5, 1, "user_6", "user_6")]
    [InlineData(13, 2, "user_14", "user_15")]
    public void GetAllClients_WithCountBiggerThanZero_ReturnsClientGetAll(int from, int count, string startName, string endName)
    {
        // arrange
        var users = _GetTestUsers();
        _users.AddRange(users);

        itemsSetup(x => x.restaurantTable);

        // act
        var result = _sut.GetAllClients(from, count);

        // assert
        result.Content.First().first_name.Should().Be(startName);
        result.Content.Last().first_name.Should().Be(endName);
        result.Content.Count.Should().Be(count);
    }

    [Theory]
    [InlineData(2, "user_3", "user_15")]
    [InlineData(0, "user_1", "user_15")]
    [InlineData(14, "user_15", "user_15")]
    public void GetAllClients_WithCountEqualsZero_ReturnsClientGetAll(int from, string firstName, string lastName)
    {
        // arrange
        var users = _GetTestUsers();
        _users.AddRange(users);

        itemsSetup(x => x.restaurantTable);

        // act
        var result = _sut.GetAllClients(from, 0);

        // assert
        result.Content.First().first_name.Should().Be(firstName);
        result.Content.Count.Should().Be(users.Count - from);
        result.Content.Last().first_name.Should().Be(lastName);
    }

    [Fact]
    public async Task DeleteClientWithAdmin_WithUserId_DeletesUser()
    {
        // arrange
        var user = Generator.GenerateUser();
        _users.Add(user);

        var restaurant = Generator.GenerateRestaurant(user.Id, RestaurantStatus.Verified);

        var restaurants = new List<RestaurantTable> { restaurant };

        var items = Generator.GenerateFoodItems(restaurant.Id, 3);

        var couriers = new List<CourierTable>
        {
            Generator.GenerateCourier(user.Id)
        };

        _context.Setup(x => x.restaurantTable).ReturnsDbSet(restaurants);
        _context.Setup(x => x.restaurantFoodItemsTable).ReturnsDbSet(items);
        _context.Setup(x => x.courierTable).ReturnsDbSet(couriers);

        _context.Setup(x => x.restaurantTable.Remove(It.IsAny<RestaurantTable>()))
            .Callback<RestaurantTable>(x => restaurants.Remove(x));
        _context.Setup(x => x.restaurantFoodItemsTable.Remove(It.IsAny<RestaurantFoodItemsTable>()))
            .Callback<RestaurantFoodItemsTable>(x => items.Remove(x));
        _context.Setup(x => x.courierTable.Remove(It.IsAny<CourierTable>()))
            .Callback<CourierTable>(x => couriers.Remove(x));

        // act
        await _sut.DeleteClientWithAdmin(user.Id);

        // assert
        _users.Count.Should().Be(0);
        restaurants.Count.Should().Be(0);
        items.Count.Should().Be(0);
        couriers.Count.Should().Be(0);
    }

    [Fact]
    public async Task DeleteClientWithAdmin_WithNonExistentUser_ThrowsException()
    {
        // arrange
        var id = Guid.NewGuid();

        // act
        Func<Task> act = async () => await _sut.DeleteClientWithAdmin(id);

        // assert
        await act.Should().ThrowAsync<Exception>().WithMessage("user_not_found");
    }

    [Fact]
    public async Task UserUpdateFromTelegram_WithCorrectData_ReturnsSuccess()
    {
        // arrange
        var user = Generator.GenerateUser();
        _users.Add(user);

        var first_name = "Artem";
        var last_name = "Stepanov";
        var address = "Moscow";
        var photo_url = "photo_url";
        var username = "@username";

        var dto = new ClientUpdate
        {
            id = user.telegram_id,
            first_name = first_name,
            last_name = last_name,
            address = address,
            photo_url = photo_url,
            username = username
        };

        // act
        await _sut.UserUpdateFromTelegram(dto);

        // assert
        user.first_name.Should().Be(first_name);
        user.last_name.Should().Be(last_name);
        user.address.Should().Be(address);
        user.photo_url.Should().Be(photo_url);
        user.username.Should().Be(username);
    }

    [Fact]
    public async Task UserUpdateFromTelegram_WithNonExistentUser_ThrowsException()
    {
        // arrange
        var first_name = "Artem";
        var last_name = "Stepanov";
        var address = "Moscow";
        var photo_url = "photo_url";
        var username = "@username";

        var dto = new ClientUpdate
        {
            id = 5782903523,
            first_name = first_name,
            last_name = last_name,
            address = address,
            photo_url = photo_url,
            username = username
        };

        // act
        Func<Task> act = async () => await _sut.UserUpdateFromTelegram(dto);

        // assert
        await act.Should().ThrowAsync<Exception>().WithMessage("user_not_found");
    }

    [Fact]
    public void InfoClientDatabase_WithCorrectUserGuidWithoutRestaurants_ReturnsSuccess()
    {
        // arrange
        var user = Generator.GenerateUser();
        _users.Add(user);

        itemsSetup(x => x.restaurantTable);

        // act
        var result = _sut.InfoClientDatabase(user.Id);

        // assert
        result.Id.Should().Be(user.Id);
        result.restaurant_own.Should().BeEmpty();
    }

    [Fact]
    public void InfoClientDatabase_WithCorrectUserGuidWithRestaurants_ReturnsSuccess()
    {
        // arrange
        var user = Generator.GenerateUser();
        _users.Add(user);

        var rests = itemsSetup(x => x.restaurantTable);

        rests.Add(Generator.GenerateRestaurant(user.Id, RestaurantStatus.Verified));
        rests.Add(Generator.GenerateRestaurant(user.Id, RestaurantStatus.Verified));

        // act
        var result = _sut.InfoClientDatabase(user.Id);

        // assert
        result.Id.Should().Be(user.Id);
        result.restaurant_own.Count.Should().Be(2);
    }

    [Fact]
    public void InfoClientDatabase_WithNonExistentUser_ReturnsNull()
    {
        // arrange
        var id = Guid.NewGuid();

        // act
        var result = _sut.InfoClientDatabase(id);

        // assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AddBasketItem_WithCorrectData_AddsItemToBasket()
    {
        // arrange
        var user = Generator.GenerateUser();
        _users.Add(user);

        var rests = itemsSetup(x => x.restaurantTable);

        var mainRest = Generator.GenerateRestaurant(Guid.NewGuid(), RestaurantStatus.Verified);
        rests.Add(mainRest);

        var foods = itemsSetup(x => x.restaurantFoodItemsTable);

        var food1 = Generator.GenerateFoodItem(mainRest.Id);

        foods.Add(food1);

        var baskets = itemsSetup(x => x.basketTable, 
            add: x => x.basketTable.Add(It.IsAny<BasketTable>()));

        var dto = new Basket_Add
        {
            user_id = user.Id,
            food_item_id = food1.Id
        };

        // act
        await _sut.AddBasketItem(dto);

        // assert
        baskets.Count.Should().Be(1);
        baskets.First().food_item_id.Should().Be(food1.Id);
        baskets.First().user_id.Should().Be(user.Id);
    }

    [Fact]
    public async Task AddBasketItem_WithPassingFoodItemWithOtherRestaurant_ClearsBasketAndAddsNewFood()
    {
        // arrange
        var user = Generator.GenerateUser();
        _users.Add(user);

        var rests = itemsSetup(x => x.restaurantTable);

        var mainRest = Generator.GenerateRestaurant(Guid.NewGuid(), RestaurantStatus.Verified);
        rests.Add(mainRest);

        var secondsRest = Generator.GenerateRestaurant(Guid.NewGuid(), RestaurantStatus.Verified);
        rests.Add(secondsRest);

        var foods = itemsSetup(x => x.restaurantFoodItemsTable);

        var food1 = Generator.GenerateFoodItem(mainRest.Id);
        foods.Add(food1);

        var food2 = Generator.GenerateFoodItem(secondsRest.Id);
        foods.Add(food2);

        var baskets = itemsSetup(x => x.basketTable,
            add: x => x.basketTable.Add(It.IsAny<BasketTable>()),
            remove: x => x.basketTable.Remove(It.IsAny<BasketTable>()));
        baskets.Add(new BasketTable { user_id = user.Id, food_item_id = food1.Id });

        var dto = new Basket_Add
        {
            user_id = user.Id,
            food_item_id = food2.Id
        };

        // act
        await _sut.AddBasketItem(dto);

        // assert
        baskets.Count.Should().Be(1);
        baskets.First().food_item_id.Should().Be(food2.Id);
        baskets.First().user_id.Should().Be(user.Id);
    }

    [Fact]
    public async Task AddBasketItem_WithNonExistentUser_ThrowsException()
    {
        // arrange
        var rests = itemsSetup(x => x.restaurantTable);
        var rest = Generator.GenerateRestaurant(Guid.NewGuid(), RestaurantStatus.Verified);
        rests.Add(rest);

        var foods = itemsSetup(x => x.restaurantFoodItemsTable);
        var food = Generator.GenerateFoodItem(rest.Id);
        foods.Add(food);

        var baskets = itemsSetup(x => x.basketTable,
            add: x => x.basketTable.Add(It.IsAny<BasketTable>()),
            remove: x => x.basketTable.Remove(It.IsAny<BasketTable>()));

        var dto = new Basket_Add
        {
            user_id = Guid.NewGuid(),
            food_item_id = food.Id
        };

        // act
        Func<Task> act = async () => await _sut.AddBasketItem(dto);

        // assert
        await act.Should().ThrowAsync<Exception>().WithMessage("user_not_found");
    }

    [Fact]
    public async Task AddBasketItem_WithNonExistentFood_ThrowsException()
    {
        // arrange
        var user = Generator.GenerateUser();
        _users.Add(user);

        var rests = itemsSetup(x => x.restaurantTable);
        var foods = itemsSetup(x => x.restaurantFoodItemsTable);
        var baskets = itemsSetup(x => x.basketTable,
            add: x => x.basketTable.Add(It.IsAny<BasketTable>()),
            remove: x => x.basketTable.Remove(It.IsAny<BasketTable>()));

        var dto = new Basket_Add
        {
            user_id = user.Id,
            food_item_id = Guid.NewGuid()
        };

        // act
        Func<Task> act = async () => await _sut.AddBasketItem(dto);

        // assert
        await act.Should().ThrowAsync<Exception>().WithMessage("food_item_not_found");
    }

    [Fact]
    public async Task GetBasketItems_WithCorrectUserIdAndEmptyBasket_ReturnsEmptyBasket()
    {
        // arrange
        var user = Generator.GenerateUser();
        _users.Add(user);

        itemsSetup(x => x.basketTable);

        // act
        var result = await _sut.GetBasketItems(user.Id);

        // assert
        result.basketInfo.count.Should().Be(0);
        result.basketInfo.totalPrice.Should().Be(0);
        result.basketItem.Count.Should().Be(0);
    }

    [Fact]
    public async Task GetBasketItems_WithCorrectUserIdAndSomeFoodInBasket_ReturnsBasket()
    {
        // arrange
        var user = Generator.GenerateUser();
        _users.Add(user);

        var restId = Guid.NewGuid();

        var foods = itemsSetup(x => x.restaurantFoodItemsTable);
        var food1 = Generator.GenerateFoodItem(restId);
        var food2 = Generator.GenerateFoodItem(restId);
        foods.Add(food1);
        foods.Add(food2);

        var basket = itemsSetup(x => x.basketTable);
        basket.Add(Generator.GenBasket(user.Id, food1.Id));
        basket.Add(Generator.GenBasket(user.Id, food1.Id));
        basket.Add(Generator.GenBasket(user.Id, food2.Id));

        // act
        var result = await _sut.GetBasketItems(user.Id);

        // assert
        result.basketInfo.count.Should().Be(3);
        result.basketInfo.totalPrice.Should().Be(food1.price + food1.price + food2.price);
        result.basketItem.Count.Should().Be(3);
    }

    [Fact]
    public async Task DeleteAllBasketWrites_WithCorrectUserId_ClearsBasket()
    {
        // arrange
        var user = Generator.GenerateUser();
        _users.Add(user);

        var basket = itemsSetup(x => x.basketTable,
            remove: x => x.basketTable.Remove(It.IsAny<BasketTable>()));
        basket.Add(Generator.GenBasket(user.Id));
        basket.Add(Generator.GenBasket(user.Id));
        basket.Add(Generator.GenBasket(user.Id));

        // act
        await _sut.DeleteAllBasketWrites(user.Id);

        // assert
        basket.Count.Should().Be(0);
    }

    [Fact]
    public async Task DeleteOneBasketWrite_WithCorrectUserId_ClearsBasket()
    {
        // arrange
        var user = Generator.GenerateUser();
        _users.Add(user);

        var basket = itemsSetup(x => x.basketTable,
            remove: x => x.basketTable.Remove(It.IsAny<BasketTable>()));

        var toDelete = Generator.GenBasket(user.Id);
        basket.Add(toDelete);
        basket.Add(Generator.GenBasket(user.Id));
        basket.Add(Generator.GenBasket(user.Id));

        // act
        await _sut.DeleteOneBasketWrite(user.Id, basket[0].Id);

        // assert
        basket.Count.Should().Be(2);
        basket.First().Id.Should().NotBe(toDelete.Id);
    }

    [Fact]
    public async Task DeleteOneBasketWrite_WithNonExistentBasket_ThrowsException()
    {
        // arrange
        var user = Generator.GenerateUser();
        _users.Add(user);

        itemsSetup(x => x.basketTable);

        // act
        Func<Task> act = async () => await _sut.DeleteOneBasketWrite(user.Id, Guid.NewGuid());

        // assert
        await act.Should().ThrowAsync<Exception>().WithMessage("write_not_founded");
    }

    [Fact]
    public async Task CreateRequestRestaurantFromUser_WithCorrectData_ReturnsNewRequestId()
    {
        // arrange
        var user = Generator.GenerateUser();
        _users.Add(user);

        var rests = itemsSetup(x => x.restaurantTable,
            add: x => x.restaurantTable.Add(It.IsAny<RestaurantTable>()));
        var requests = itemsSetup(x => x.requestTable,
            add: x => x.requestTable.Add(It.IsAny<RequestTable>()));

        var dto = Generator.GenerateRestaurant(Guid.NewGuid(), RestaurantStatus.Unverified)
            .ToRequestDto("description");

        // act
        var result = await _sut.CreateRequestRestaurantFromUser(user.Id, dto);

        // assert
        rests.Count.Should().Be(1);
        requests.Count.Should().Be(1);
        requests.First().Id.Should().Be(result);
    }

    [Fact]
    public async Task CreateRequestRestaurantFromUser_WithHaving6RestaurantsAndPassingCorrectData_ThrowsException()
    {
        // arrange
        var user = Generator.GenerateUser();
        _users.Add(user);

        var rests = itemsSetup(x => x.restaurantTable,
            add: x => x.restaurantTable.Add(It.IsAny<RestaurantTable>()));
        rests.AddRange(Generator.GenerateRestaurants(user.Id, 6));

        var requests = itemsSetup(x => x.requestTable,
            add: x => x.requestTable.Add(It.IsAny<RequestTable>()));

        var dto = Generator.GenerateRestaurant(Guid.NewGuid(), RestaurantStatus.Unverified)
            .ToRequestDto("description");

        // act
        Func<Task> act = async() => await _sut.CreateRequestRestaurantFromUser(user.Id, dto);

        // assert
        await act.Should().ThrowAsync<Exception>().WithMessage("max_restaurants_detected");
    }

    [Fact]
    public async Task CreateRequestCourierFromUser_WithCorrectData_ReturnsRequestId()
    {
        // arrange
        var user = Generator.GenerateUser();
        _users.Add(user);

        var couriers = itemsSetup(x => x.courierTable,
            add: x => x.courierTable.Add(It.IsAny<CourierTable>()));
        var requests = itemsSetup(x => x.requestTable,
            add: x => x.requestTable.Add(It.IsAny<RequestTable>()));

        // act
        var result = await _sut.CreateRequestCourierFromUser(user.Id, "A853BC84", "desc");

        // assert
        couriers.Count.Should().Be(1);
        requests.Count.Should().Be(1);
        requests.First().Id.Should().Be(result);
    }

    [Fact]
    public async Task CreateRequestCourierFromUser_WithBeeingACourierAndPassingCorrectData_ThrowsException()
    {
        // arrange
        var user = Generator.GenerateUser();
        _users.Add(user);

        var couriers = itemsSetup(x => x.courierTable,
            add: x => x.courierTable.Add(It.IsAny<CourierTable>()));
        couriers.Add(Generator.GenerateCourier(user.Id));

        var requests = itemsSetup(x => x.requestTable,
            add: x => x.requestTable.Add(It.IsAny<RequestTable>()));

        // act
        Func<Task> act = async () => await _sut.CreateRequestCourierFromUser(user.Id, "A853BC84", "desc");

        // assert
        await act.Should().ThrowAsync<Exception>().WithMessage("max_courier_detected");
    }

    [Fact]
    public async Task AcceptRequestRestaurantFromAdmin_WithCorrectData_VerifiesRestaurantAndRemoveRequest()
    {
        // arrange
        var user = Generator.GenerateUser();
        _users.Add(user);

        var rests = itemsSetup(x => x.restaurantTable);
        rests.Add(Generator.GenerateRestaurant(user.Id, RestaurantStatus.Unverified));

        var requests = itemsSetup(x => x.requestTable,
            remove: x => x.requestTable.Remove(It.IsAny<RequestTable>()));

        requests.Add(Generator.GenRestaurantRequest(user.Id, rests[0].Id));

        // act
        await _sut.AcceptRequestRestaurantFromAdmin(requests[0].Id);

        // assert
        rests[0].status.Should().Be(RestaurantStatus.Verified);
        requests.Count.Should().Be(0);
    }

    [Fact]
    public async Task AcceptRequestRestaurantFromAdmin_WithNonExistentRequest_ThrowsException()
    {
        // arrange
        var id = Guid.NewGuid();

        itemsSetup(x => x.requestTable);

        // act
        Func<Task> act = async() => await _sut.AcceptRequestRestaurantFromAdmin(id);

        // assert
        await act.Should().ThrowAsync<Exception>().WithMessage("request_not_found");
    }

    [Fact]
    public async Task AcceptRequestRestaurantFromAdmin_WithNonExistentRestaurant_ThrowsException()
    {
        // arrange
        var user = Generator.GenerateUser();
        _users.Add(user);

        var requests = itemsSetup(x => x.requestTable);
        itemsSetup(x => x.restaurantTable);

        requests.Add(Generator.GenRestaurantRequest(user.Id, Guid.NewGuid()));

        // act
        Func<Task> act = async () =>  await _sut.AcceptRequestRestaurantFromAdmin(requests[0].Id);

        // assert
        await act.Should().ThrowAsync<Exception>().WithMessage("restaurant_not_found");
    }

    [Fact]
    public async Task AcceptRequestCourierFromAdmin_WithCorrectData_VerifiesCourierAndRemoveRequest()
    {
        // arrange
        var user = Generator.GenerateUser();
        _users.Add(user);

        var couriers = itemsSetup(x => x.courierTable);
        couriers.Add(Generator.GenerateCourier(user.Id, CourierStatus.Unverified));

        var requests = itemsSetup(x => x.requestTable,
            remove: x => x.requestTable.Remove(It.IsAny<RequestTable>()));

        requests.Add(Generator.GenCourierRequest(user.Id, couriers[0].Id));

        // act
        await _sut.AcceptRequestCourierFromAdmin(requests[0].Id);

        // assert
        couriers[0].status.Should().Be(CourierStatus.IsInactive);
        user.roles.Should().Contain("Courier");
        requests.Count.Should().Be(0);

        //todo: more tests
    }

    [Fact]
    public async Task RejectRequestRestaurantFromAdmin_WithCorrectData_RemovesRestaurantAndRequest()
    {
        // arrange
        var user = Generator.GenerateUser();
        var rest = Generator.GenerateRestaurant(user.Id, RestaurantStatus.Unverified);
        var request = Generator.GenRestaurantRequest(user.Id, rest.Id);

        _users.Add(user);
        var rests = itemsSetup(x => x.restaurantTable,
            remove: x => x.restaurantTable.Remove(any<RestaurantTable>()));
        rests.Add(rest);

        var requests = itemsSetup(x => x.requestTable,
            remove: x => x.requestTable.Remove(any<RequestTable>()));
        requests.Add(request);

        // act
        await _sut.RejectRequestRestaurantFromAdmin(request.Id);

        // assert
        rests.Count.Should().Be(0);
        requests.Count.Should().Be(0);
    }

    [Fact]
    public async Task RejectRequestRestaurantFromAdmin_WithNonExistentRequest_ThrowsException()
    {
        // arrange
        var requests = itemsSetup(x => x.requestTable);

        // act
        Func<Task> act = async () => await _sut.RejectRequestRestaurantFromAdmin(Guid.NewGuid());

        // assert
        await act.Should().ThrowAsync<Exception>().WithMessage("request_not_found");
    }

    [Fact]
    public async Task RejectRequestRestaurantFromAdmin_WithNonExistentRestaurant_ThrowsException()
    {
        // arrange
        var user = Generator.GenerateUser();
        var request = Generator.GenRestaurantRequest(user.Id, Guid.NewGuid());

        _users.Add(user);
        itemsSetup(x => x.restaurantTable);

        var requests = itemsSetup(x => x.requestTable,
            remove: x => x.requestTable.Remove(any<RequestTable>()));
        requests.Add(request);

        // act
        Func<Task> act = async () => await _sut.RejectRequestRestaurantFromAdmin(request.Id);

        // assert
        await act.Should().ThrowAsync<Exception>().WithMessage("restaurant_not_found");
    }

    [Fact]
    public async Task RejectRequestCourierFromAdmin_WithCorrectData_RemovesCourierAndRequest()
    {
        // arrange
        var user = Generator.GenerateUser();
        var courier = Generator.GenerateCourier(user.Id, CourierStatus.Unverified);
        var request = Generator.GenCourierRequest(user.Id, courier.Id);

        _users.Add(user);
        var couriers = itemsSetup(x => x.courierTable,
            remove: x => x.courierTable.Remove(any<CourierTable>()));
        couriers.Add(courier);

        var requests = itemsSetup(x => x.requestTable,
            remove: x => x.requestTable.Remove(any<RequestTable>()));
        requests.Add(request);

        // act
        await _sut.RejectRequestCourierFromAdmin(request.Id);

        // assert
        couriers.Count.Should().Be(0);
        requests.Count.Should().Be(0);
    }

    [Fact]
    public async Task RejectRequestCourierFromAdmin_WithNonExistentRequest_ThrowsException()
    {
        // arrange
        itemsSetup(x => x.requestTable);

        // act
        Func <Task> act = async () => await _sut.RejectRequestCourierFromAdmin(Guid.NewGuid());

        // assert
        await act.Should().ThrowAsync<Exception>().WithMessage("request_not_found");
    }

    [Fact]
    public async Task RejectRequestCourierFromAdmin_WithNonExistentCourier_ThrowsException()
    {
        // arrange
        var user = Generator.GenerateUser();
        var request = Generator.GenCourierRequest(user.Id, Guid.NewGuid());

        _users.Add(user);
        itemsSetup(x => x.courierTable);

        var requests = itemsSetup(x => x.requestTable,
            remove: x => x.requestTable.Remove(any<RequestTable>()));
        requests.Add(request);

        // act
        Func<Task> act = async () => await _sut.RejectRequestCourierFromAdmin(request.Id);

        // assert
        await act.Should().ThrowAsync<Exception>().WithMessage("courier_not_found");
    }

    [Fact]
    public void GetOnlyMeRequestsRestaurant_WithExistentUserAndSomeRequestsAndRestaurants_ReturnsRequestInfoRestaurants()
    {
        // arrange
        var user = Generator.GenerateUser();
        var rest = Generator.GenerateRestaurant(user.Id, RestaurantStatus.Verified);
        var request = Generator.GenRestaurantRequest(user.Id, rest.Id);

        var rest2 = Generator.GenerateRestaurant(user.Id, RestaurantStatus.Verified);
        var request2 = Generator.GenRestaurantRequest(user.Id, rest2.Id);

        _users.Add(user);
        itemsSetup(x => x.requestTable).AddItems(request, request2);
        itemsSetup(x => x.restaurantTable).AddItems(rest, rest2);

        // act
        var result = _sut.GetOnlyMeRequestsRestaurant(user.Id);

        // assert
        result.Count.Should().Be(2);
        result[0].request_id.Should().Be(request.Id);
        result[0].client_info.Id.Should().Be(user.Id);

        result[1].request_id.Should().Be(request2.Id);
        result[1].client_info.Id.Should().Be(user.Id);
    }

    [Fact]
    public void GetOnlyMeRequestsRestaurant_WithExistentUserAndEmptyRestaurants_ReturnsEmptyList()
    {
        // arrange
        var user = Generator.GenerateUser();

        _users.Add(user);
        itemsSetup(x => x.requestTable);
        itemsSetup(x => x.restaurantTable);

        // act
        var result = _sut.GetOnlyMeRequestsRestaurant(user.Id);

        // assert
        result.Count.Should().Be(0);
    }

    [Fact]
    public void GetOnlyMeRequestCourier_WithExistentUserAndSomeCourierAndRequest_ReturnsRequestInfoCouriers()
    {
        // arrange
        var user = Generator.GenerateUser();
        var courier = Generator.GenerateCourier(user.Id, CourierStatus.IsInactive);
        var request = Generator.GenCourierRequest(user.Id, courier.Id);

        _users.Add(user);
        itemsSetup(x => x.requestTable).AddItem(request);
        itemsSetup(x => x.courierTable).AddItem(courier);

        // act
        var result = _sut.GetOnlyMeRequestCourier(user.Id);

        // assert
        result.request_id.Should().Be(request.Id);
        result.client_info.Id.Should().Be(user.Id);
    }

    [Fact]
    public void GetOnlyMeRequestCourier_WithExistentUserWithoutCourier_ReturnsNull()
    {
        // arrange
        var user = Generator.GenerateUser();

        _users.Add(user);
        itemsSetup(x => x.requestTable);
        itemsSetup(x => x.courierTable);

        // act
        var result = _sut.GetOnlyMeRequestCourier(user.Id);

        // assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetAllRequestsForAdmin_WithSomeRequests_ReturnsListOfRequests()
    {
        // arrange
        var user = Generator.GenerateUser();
        _users.Add(user);

        var rest1 = Generator.GenerateRestaurant(user.Id, RestaurantStatus.Verified);
        var rest2 = Generator.GenerateRestaurant(user.Id, RestaurantStatus.Verified);

        var restRequest1 = Generator.GenRestaurantRequest(user.Id, rest1.Id);
        var restRequest2 = Generator.GenRestaurantRequest(user.Id, rest2.Id);

        var courier1 = Generator.GenerateCourier(user.Id);
        var courier2 = Generator.GenerateCourier(user.Id);

        var courierRequest1 = Generator.GenCourierRequest(user.Id, courier1.Id);
        var courierRequest2 = Generator.GenCourierRequest(user.Id, courier2.Id);

        itemsSetup(x => x.requestTable).AddItems(restRequest1, restRequest2, courierRequest1, courierRequest2);
        itemsSetup(x => x.courierTable).AddItems(courier1, courier2);
        itemsSetup(x => x.restaurantTable).AddItems(rest1, rest2);

        // act
        var result = _sut.GetAllRequestsForAdmin();

        // assert
        result.courier_requests.Count.Should().Be(2);
        result.restaurant_requests.Count.Should().Be(2);

        result.courier_requests[0].request_id.Should().Be(courierRequest1.Id);
        result.courier_requests[1].request_id.Should().Be(courierRequest2.Id);

        result.courier_requests[0].client_info.Id.Should().Be(user.Id);
        result.courier_requests[1].client_info.Id.Should().Be(user.Id);

        result.restaurant_requests[0].request_id.Should().Be(restRequest1.Id);
        result.restaurant_requests[1].request_id.Should().Be(restRequest2.Id);

        result.restaurant_requests[0].client_info.Id.Should().Be(user.Id);
        result.restaurant_requests[1].client_info.Id.Should().Be(user.Id);
    }

    [Fact]
    public void GetAllRequestsForAdmin_WithZeroRequests_ReturnsEmptyLists()
    {
        // arrange
        itemsSetup(x => x.requestTable);
        itemsSetup(x => x.courierTable);
        itemsSetup(x => x.restaurantTable);

        // act
        var result = _sut.GetAllRequestsForAdmin();

        // assert
        result.courier_requests.Count.Should().Be(0);
        result.restaurant_requests.Count.Should().Be(0);
    }

    //[Fact]
    //public void GetAllFrozenEntities_WithFrozenRestaurantsAndCouriers_ReturnsListsOfFrozenRestaurantsAndCouriers()
    //{
    //    // arrange
    //    var user = Generator.GenerateUser();
    //    _users.Add(user);

    //    itemsSetup(x => x.restaurantTable).AddRange(Generator.GenerateRestaurants(user.Id, RestaurantStatus.Frozen, 3));
    //    //itemsSetup(x => x.restaurantTable)
    //    //todo: couriers

    //    // act

    //    // assert
    //}
}