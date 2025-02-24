using Microsoft.AspNetCore.Mvc;
using ORM_Components.DTO.RestaurantAPI;
using RestaurantAPI.Model.Services;

namespace RestaurantAPI.Model.Interface
{
    public interface IPhotoServices
    {
        public Task ValidationTokenPhoto([FromForm] Photos_DTO photo_DTO, string token);

    }
}
