using Microsoft.AspNetCore.Mvc;
using ORM_Components.DTO.RestaurantAPI;
using ORM_Components.Tables;

namespace RestaurantAPI.Model.Interface
{
    public interface IRestaurantFoodItemsServices
    {
        Task<IRestaurantFoodItemsServices> AddRestaurantFoodItems([FromBody] RestaurantFoodItems_DTO restaurantFoodItems_DTO);
        Task<IRestaurantFoodItemsServices> DeleteRestaurantFoodItems(Guid id);
        Task<IRestaurantFoodItemsServices> DeleteAllRestaurantFoodItems();
        Task<List<RestaurantFoodItemsTable>> GetRestaurantFoodItems(Guid restaurantId);
        Task<List<RestaurantFoodItemsTable>> GetAllRestaurantFoodItems();
        Task<string> PutRestaurantFoodItems([FromBody] RestaurantFoodItems_DTO restaurantFoodItems_DTO, Guid food_Id);
    }
}
