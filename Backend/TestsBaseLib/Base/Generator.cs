using Bogus;
using ORM_Components.Tables;
using ORM_Components.Tables.Helpers;
using System.Linq.Expressions;

namespace TestsBaseLib.Base;

public static class Generator
{
    private static Faker<UserTable> _GetUserFaker(string roles)
    {
        var faker = new Faker<UserTable>();
        faker.RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.username, f => f.Random.Word())
            .RuleFor(x => x.first_name, f => f.Name.FirstName())
            .RuleFor(x => x.last_name, f => f.Name.FirstName())
            .RuleFor(x => x.address, f => f.Address.City())
            .RuleFor(x => x.roles, _ => roles.Split(" "))
            .RuleFor(x => x.money_value, _ => 0)
            .RuleFor(x => x.telegram_id, f => f.Random.Long(0, long.MaxValue))
            .RuleFor(x => x.telegram_chat_id, f => f.Random.Long(0, long.MaxValue));

        return faker;
    }

    public static UserTable GenerateUser(string roles = "Client") =>
        _GetUserFaker(roles).Generate();

    public static UserTable GenerateUserWithName(string first_name, string roles = "Client") =>
        _GetUserFaker(roles).RuleFor(x => x.first_name, _ => first_name).Generate();

    public static List<UserTable> GenerateUsers(int count, string roles = "Client") =>
        _GetUserFaker(roles).Generate(count);

    private static Faker<RestaurantTable> _GetRestaurantFaker(Guid user_id, RestaurantStatus status)
    {
        var faker = new Faker<RestaurantTable>();
        faker.RuleFor(x => x.Id, _ => Guid.NewGuid())
            .RuleFor(x => x.user_id, _ => user_id)
            .RuleFor(x => x.phone_number, f => f.Phone.PhoneNumber())
            .RuleFor(x => x.imagePath, f => f.Random.Word())
            .RuleFor(x => x.restaurantName, f => f.Random.Word())
            .RuleFor(x => x.description, f => f.Random.Words(10))
            .RuleFor(x => x.address, f => f.Address.City())
            .RuleFor(x => x.close_time, f => f.Date.BetweenTimeOnly(TimeOnly.Parse("7:00"), TimeOnly.Parse("23:59")).ToShortTimeString())
            .RuleFor(x => x.open_time, (f, x) => f.Date.BetweenTimeOnly(TimeOnly.Parse(x.close_time), TimeOnly.Parse(x.close_time).AddHours(-12)).ToShortTimeString())
            .RuleFor(x => x.status, _ => status);
        
        return faker;
    }
    public static RestaurantTable GenerateRestaurant(Guid user_id, RestaurantStatus status) =>
        _GetRestaurantFaker(user_id, status).Generate();

    public static List<RestaurantTable> GenerateRestaurants(Guid user_id, int count) =>
        _GetRestaurantFaker(user_id, RestaurantStatus.Unverified).Generate(count);

    public static RequestTable GenRestaurantRequest(Guid user_id, Guid rest_id)
    {
        var faker = new Faker<RequestTable>();
        faker.RuleFor(x => x.Id, _ => Guid.NewGuid())
            .RuleFor(x => x.user_id, _ => user_id)
            .RuleFor(x => x.description, f => f.Random.Word())
            .RuleFor(x => x.restaurant_id, _ => rest_id)
            .RuleFor(x => x.time_add, f => f.Date.Between(DateTime.UtcNow.AddDays(-5), DateTime.UtcNow.AddDays(5)));

        return faker.Generate();
    }

    public static RequestTable GenCourierRequest(Guid user_id, Guid courier_id)
    {
        var faker = new Faker<RequestTable>();
        faker.RuleFor(x => x.Id, _ => Guid.NewGuid())
            .RuleFor(x => x.user_id, _ => user_id)
            .RuleFor(x => x.description, f => f.Random.Word())
            .RuleFor(x => x.courier_id, _ => courier_id)
            .RuleFor(x => x.time_add, f => f.Date.Between(DateTime.UtcNow.AddDays(-5), DateTime.UtcNow.AddDays(5)));

        return faker.Generate();
    }

    public static BasketTable GenBasket(Guid user_id, Guid food_id) => new BasketTable
    {
        Id = Guid.NewGuid(),
        user_id = user_id,
        food_item_id = food_id
    };

    public static BasketTable GenBasket(Guid user_id) => GenBasket(user_id, Guid.NewGuid());

    public static RestaurantFoodItemsTable GenerateFoodItem(Guid restaurant_id)
    {
        var items = GenerateFoodItems(restaurant_id, 1);
        return items.First();
    }

    public static List<RestaurantFoodItemsTable> GenerateFoodItems(Guid restaurant_id, int count)
    {
        var faker = new Faker<RestaurantFoodItemsTable>();
        faker.RuleFor(x => x.Id, _ => Guid.NewGuid())
            .RuleFor(x => x.restaurant_id, _ => restaurant_id)
            .RuleFor(x => x.price, f => f.Random.Number(200))
            .RuleFor(x => x.weight, f => f.Random.Number(1, 100))
            .RuleFor(x => x.calories, f => f.Random.Number(50, 3000))
            .RuleFor(x => x.image, f => f.Random.Word())
            .RuleFor(x => x.name, f => f.Random.Word());

        return faker.Generate(count);
    }

    public static OrderTable GenerateOrder(Guid client_id, Guid restaurant_id, OrderStatus status, Guid? courier_id = null)
    {
        var faker = new Faker<OrderTable>();
        faker.RuleFor(x => x.Id, _ => Guid.NewGuid())
            .RuleFor(x => x.total_price, f => f.Random.Number(10, 5000))
            .RuleFor(x => x.order_date, f => f.Date.Between(DateTime.UtcNow.AddDays(-7), DateTime.UtcNow))
            .RuleFor(x => x.courier_id, _ => courier_id)
            .RuleFor(x => x.restaurant_id, _ => restaurant_id)
            .RuleFor(x => x.client_id, _ => client_id)
            .RuleFor(x => x.status, _ => status);

        return faker.Generate();
    }

    public static OrderTable GenerateOrder(OrderStatus status)
    {
        return GenerateOrder(Guid.NewGuid(), Guid.NewGuid(), status);
    }

    public static CourierTable GenerateCourier(Guid user_id, CourierStatus status)
    {
        var faker = new Faker<CourierTable>();
        faker.RuleFor(x => x.Id, _ => Guid.NewGuid())
            .RuleFor(x => x.status, _ => status)
            .RuleFor(x => x.car_number, f => f.Random.String2(8, "1234567890ABCEHOP"))
            .RuleFor(x => x.userId, _ => user_id);

        return faker.Generate();
    }

    public static CourierTable GenerateCourier(Guid user_id)
    {
        return GenerateCourier(user_id, CourierStatus.IsInactive);
    }
}
