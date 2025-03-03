using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ORM_Components.DTO.RestaurantAPI;
using ORM_Components.Tables;

namespace RestaurantAPI.Model.Interface
{
   
    public interface IPhotoServices
    {
        Task<IPhotoServices> AddPhotos([FromForm] Photos_DTO photo_DTO);
        Task<IPhotoServices> RemovePhoto(int id);
        Task<IPhotoServices> RemoveAllPhotos();
        Task<List<RestaurantTable>> GetPhotos();
        Task<List<RestaurantTable>> GetAllPhotos(Guid restaurantId);
    }
}
