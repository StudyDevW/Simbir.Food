using Microsoft.AspNetCore.Http;
using Middleware_Components.JWT.DTO.Token;
using Middleware_Components.Services;
using Moq;
using ORM_Components.DTO.RestaurantAPI;
using ORM_Components.Tables;
using ORM_Components.Tables.Helpers;
using Moq.Language.Flow;
using Microsoft.AspNetCore.Mvc;
using ORM_Components.DTO.ClientAPI;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

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
                    userRoles = roles.Split(" ").ToList(),
                }
            });
    }

    public static AuthAddUser ToDto(this UserTable table, string device) => new AuthAddUser
    {
        first_name = table.first_name,
        address = table.address,
        chat_id = table.telegram_chat_id,
        device = device,
        id = table.telegram_id,
        is_bot = false,
        last_name = table.last_name,
        photo_url = null,
        roles = table.roles,
        username = table.username
    };

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
        return new Restaurants_DTO(table.Id,
                                   table.user_id,
                                   table.restaurantName,
                                   table.address,
                                   table.phone_number,
                                   table.status,
                                   table.description,
                                   table.imagePath,
                                   table.open_time,
                                   table.close_time);
    }

    public static RestaurantUpdate_DTO ToUpdateDto(this RestaurantTable table) => new RestaurantUpdate_DTO(
        table.restaurantName, table.address, table.phone_number, table.status, table.description, table.imagePath,
        table.open_time, table.close_time);

    /// <summary>
    /// Адаптация RestaurantTable в RestaurantAddRequest
    /// </summary>
    public static RestaurantAddRequest ToRequestDto(this RestaurantTable table, string request_description) =>
        new RestaurantAddRequest
        {
            address = table.address,
            close_time = table.close_time,
            description = table.description,
            imagePath = table.imagePath,
            open_time = table.open_time,
            phone_number = table.phone_number,
            restaurantName = table.restaurantName,
            request_description = request_description
        };

    /// <summary>
    /// Адаптация RestaurantFoodItemsTable в RestaurantFoodItems_DTO
    /// </summary>
    public static RestaurantFoodItemsDtoForCreate ToDto(this RestaurantFoodItemsTable table) =>
        new RestaurantFoodItemsDtoForCreate(table.restaurant_id, table.name, table.price, table.image, table.weight, table.calories);

    public static RestaurantFoodItemsDtoForCreate ToCreateDto(this RestaurantFoodItemsTable table) =>
        new RestaurantFoodItemsDtoForCreate(table.restaurant_id, table.name, table.price, table.image, table.weight, table.calories);

    public static RestaurantFoodItemsDtoForCreate ToCreateDto(this RestaurantFoodItemsTable table, int price, int weight, int calories, string name) =>
        new RestaurantFoodItemsDtoForCreate(table.restaurant_id, name, price, table.image, weight, calories);

    public static RestaurantFoodItemsDtoForUpdate ToUpdateDto(this RestaurantFoodItemsTable table) =>
        new RestaurantFoodItemsDtoForUpdate(table.name, table.price, table.image, table.weight, table.calories);

    public static RestaurantFoodItemsDtoForUpdate ToUpdateDto(this RestaurantFoodItemsTable table, int price, int weight, int calories, string name) =>
        new RestaurantFoodItemsDtoForUpdate(name, price, table.image, weight, calories);

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

    public static List<T> AddItem<T>(this List<T> list, T item)
    {
        list.Add(item);
        return list;
    }

    public static List<T> AddItems<T>(this List<T> list, params T[] items)
    {
        list.AddRange(items);
        return list;
    }

    public static List<T> AddItems<T>(this List<T> list, IEnumerable<T> items)
    {
        list.AddRange(items);
        return list;
    }
}
