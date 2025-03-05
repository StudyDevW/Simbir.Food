using Microsoft.AspNetCore.Mvc;
using ORM_Components.DTO.RestaurantAPI;
using ORM_Components.Tables;

namespace RestaurantAPI.Model.Interface
{
    public interface IRestaurantFoodItemsServices
    {
        Task<string> AddRestaurantFoodItems([FromBody] RestaurantFoodItems_DTO restaurantFoodItems_DTO);
        Task<string> DeleteRestaurantFoodItems(Guid id);
        Task<string> DeleteAllRestaurantFoodItems(Guid restaurantId);
        Task<List<RestaurantFoodItemsTable>> GetRestaurantFoodItems(Guid restaurantId);
        Task<List<RestaurantFoodItemsTable>> GetAllRestaurantFoodItems();
        Task<string> PutRestaurantFoodItems([FromBody] RestaurantFoodItems_DTO restaurantFoodItems_DTO, Guid food_Id);
    }
}
