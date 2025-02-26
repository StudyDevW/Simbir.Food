using Microsoft.AspNetCore.Http;
using Middleware_Components.JWT.DTO.Token;
using Middleware_Components.Services;
using Moq;
using ORM_Components.DTO.RestaurantAPI;
using ORM_Components.Tables;
using ORM_Components.Tables.Helpers;
using Moq.Language.Flow;
using Microsoft.AspNetCore.Mvc;

namespace TestsBaseLib.Base;

public static class UnitTestsExtensions
{
    /// <summary>
    /// Заменяет метод AccessTokenValidation моком, возвращающим успешный результат
    /// </summary>
    /// <param name="chatId">Id чата телеграм</param>
    /// <param name="roles">Роли пользователя, указанные через пробел</param>
    public static void InitJwt(this Mock<IJwtService> _jwt, string chatId = "29402152", string roles = "Client")
    {
        _jwt.Setup(x => x.AccessTokenValidation(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new Token_ValidProperties
            {
                token_success = new Token_ValidSuccess
                {
                    Id = Guid.NewGuid(),
                    telegramChatId = chatId,
                    userRoles = roles.Split(" ").ToList(),
                }
            });
    }

    /// <summary>
    /// Адаптация OrderTable в Order_DTO
    /// </summary>
    public static Order_DTO ToDto(this OrderTable table)
    {
        return new Order_DTO
        {
            restaurant_id = table.restaurant_id,
            client_id = table.client_id,
            courier_id = table.courier_id,
            id = table.Id,
            order_date = table.order_date,
            status = table.status,
            total_price = table.total_price
        };
    }

    /// <summary>
    /// Адаптация RestaurantTable в Restaurants_DTO
    /// </summary>
    public static Restaurants_DTO ToDto(this RestaurantTable table)
    {
        return new Restaurants_DTO
        {
            address = table.address,
            close_time = table.close_time,
            description = table.description,
            id = table.Id,
            imagePath = table.imagePath,
            open_time = table.open_time,
            phone_number = table.phone_number,
            restaurantName = table.restaurantName,
            status = table.status,
            user_id = table.user_id,
        };
    }

    /// <summary>
    /// Адаптация RestaurantFoodItemsTable в RestaurantFoodItems_DTO
    /// </summary>
    public static RestaurantFoodItems_DTO ToDto(this RestaurantFoodItemsTable table)
    {
        return new RestaurantFoodItems_DTO
        {
            calories = table.calories,
            image = table.image,
            name = table.name,
            price = table.price,
            restaurant_id = table.restaurant_id,
            weight = table.weight
        };
    }

    /// <summary>
    /// Создает функцию, находящую элемент из списка по его Id
    /// </summary>
    private static Func<object[], ValueTask<T?>> SetupFindAsync<T>(IEnumerable<T> items)
        where T: IId
    {
        return new Func<object[], ValueTask<T?>>((x) =>
        {
            var guid = (Guid)x.First();
            var rest = items.FirstOrDefault(c => c.Id == guid);
            var task = ValueTask.FromResult(rest);
            return task;
        });
    }

    /// <summary>
    /// Расширение для Moq, возвращающее моковую функцию FindAsync
    /// </summary>
    public static IReturnsResult<T> ReturnsFindAsync<T, D>(this ISetup<T, ValueTask<D?>> setupResult, IEnumerable<D> items) 
        where T : class 
        where D : IId
    {

        return setupResult.Returns(SetupFindAsync(items));
    }

    /// <summary>
    /// Настраивает контекст мокового контроллера
    /// </summary>
    /// <param name="controller">Контроллер</param>
    public static void ConfigureContext(this ControllerBase controller)
    {
        controller.ControllerContext = new ControllerContext();
        controller.ControllerContext.HttpContext = new DefaultHttpContext();
        controller.ControllerContext.HttpContext.Request.Headers.Authorization = "auth";
    }
}
