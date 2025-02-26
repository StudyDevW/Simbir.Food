using Bogus;
using ORM_Components.Tables;
using ORM_Components.Tables.Helpers;

namespace TestsBaseLib.Base;

public static class Generator
{
    public static UserTable GenerateUser(string login, string passwordHash, string[] roles)
    {
        var faker = new Faker<UserTable>();
        faker.RuleFor(x => x.Id, f => Guid.NewGuid())
            .RuleFor(x => x.login, f => login)
            .RuleFor(x => x.password, _ => passwordHash)
            .RuleFor(x => x.name, f => f.Name.FirstName())
            .RuleFor(x => x.email, (f, x) => f.Internet.Email(x.name))
            .RuleFor(x => x.phone_number, f => f.Phone.PhoneNumber())
            .RuleFor(x => x.address, f => f.Address.City())
            .RuleFor(x => x.roles, _ => roles)
            .RuleFor(x => x.chatId, f => f.Random.String2(8, "1234567890"));

        return faker.Generate();
    }

    public static UserTable GenerateUser(string role = "Client")
    {
        return GenerateUser(Guid.NewGuid().ToString(), "pass1", new string[] { role });
    }

    public static RestaurantTable GenerateRestaurant(Guid user_id)
    {
        var faker = new Faker<RestaurantTable>();
        faker.RuleFor(x => x.Id, _ => Guid.NewGuid())
            .RuleFor(x => x.user_id, _ => user_id)
            .RuleFor(x => x.phone_number, f => f.Phone.PhoneNumber())
            .RuleFor(x => x.imagePath, f => f.Random.Word())
            .RuleFor(x => x.restaurantName, f => f.Random.Word())
            .RuleFor(x => x.description, f => f.Random.Words(10))
            .RuleFor(x => x.address, f => f.Address.City())
            .RuleFor(x => x.close_time, f => f.Date.Between(new DateTime(2016, 1, 1, 0, 0, 0, DateTimeKind.Utc), DateTime.UtcNow))
            .RuleFor(x => x.open_time, (f, x) => f.Date.Between(DateTime.UtcNow, x.close_time))
            .RuleFor(x => x.status, f => f.Random.Word());

        return faker.Generate();
    }

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
