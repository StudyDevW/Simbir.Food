using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ORM_Components.DTO.RestaurantAPI;
using ORM_Components.Tables;

namespace RestaurantAPI.Model.Interface
{
   
    public interface IPhotoServices
    {
        Task AddPhotos([FromForm] Photos_DTO photo_DTO);
        Task RemovePhoto(int id);
        Task RemoveAllPhotos();
        Task<List<RestaurantTable>> GetPhotos();
        Task<List<RestaurantTable>> GetAllPhotos(Guid restaurantId);
    }
}
