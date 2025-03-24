using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ORM_Components.DTO.RestaurantAPI;
using ORM_Components.Tables;

namespace RestaurantAPI.Model.Interface
{
   
    public interface IPhotoServices
    {
        Task AddPhotoRestaurant(PhotoDTO_Restaurant photo_DTO);
        Task AddPhotoRestaurantFoodItem(PhotoDTO_FoodItem photo_DTO);

        Task RemovePhotoFromRestaurant(Guid restaurantId);
        Task RemovePhotoFromFoodItem(Guid fooditemId);
    }
}
