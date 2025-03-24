using Microsoft.AspNetCore.Mvc;
using ORM_Components.DTO.RestaurantAPI;
using ORM_Components.Tables;

namespace RestaurantAPI.Model.Interface
{
    public interface IRestaurantFoodItemsServices
    {
        Task AddRestaurantFoodItems(RestaurantFoodItemsDtoForCreate restaurantFoodItemsDtoForCreate);
        Task DeleteRestaurantFoodItems(Guid id);
        Task DeleteAllRestaurantFoodItems(Guid restaurantId);
        Task<List<RestaurantFoodItemsDto>> GetRestaurantFoodItems(Guid restaurantId);
        Task<List<RestaurantFoodItemsDto>> GetAllRestaurantFoodItems();
        Task UpdateRestaurantFoodItems(Guid food_Id, RestaurantFoodItemsDtoForUpdate restaurantFoodItemsDtoForUpdate);
    }
}
