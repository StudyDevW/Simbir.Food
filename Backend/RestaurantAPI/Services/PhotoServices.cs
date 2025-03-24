using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Middleware_Components.Services;
using ORM_Components;
using ORM_Components.DTO.RestaurantAPI;
using ORM_Components.Tables;
using RestaurantAPI.Model.Interface;

namespace RestaurantAPI.Model.Services
{
    public class PhotoServices : IPhotoServices
    {
        private readonly DataContext _dbContext;

        public PhotoServices(DataContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddPhotoRestaurant(PhotoDTO_Restaurant photo_DTO)
        {       
            if (photo_DTO.File == null || photo_DTO.File.Length == 0)
            {
                throw new Exception("file_incorrect");
            }

            var filePath = Path.GetFullPath("uploads/" + photo_DTO.File.FileName);

            if (!Directory.Exists(Path.GetFullPath("uploads/")))
                Directory.CreateDirectory(Path.GetFullPath("uploads/"));

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await photo_DTO.File.CopyToAsync(stream);
            }

            var restaurantSelected = _dbContext.restaurantTable
                .Where(c => c.Id == photo_DTO.restaurantId).FirstOrDefault();

            if (restaurantSelected == null)
                throw new Exception("restaurant_not_found");

            restaurantSelected.imagePath = filePath;
            await _dbContext.SaveChangesAsync();
        }

        public async Task AddPhotoRestaurantFoodItem(PhotoDTO_FoodItem photo_DTO)
        {
            if (photo_DTO.File == null || photo_DTO.File.Length == 0)
            {
                throw new Exception("file_incorrect");
            }

            var filePath = Path.GetFullPath("uploads/" + photo_DTO.File.FileName);


            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await photo_DTO.File.CopyToAsync(stream);
            }

            var foodItemSelected = _dbContext.restaurantFoodItemsTable
                .Where(c => c.Id == photo_DTO.fooditemId).FirstOrDefault();

            if (foodItemSelected == null)
                throw new Exception("fooditem_not_found");

            foodItemSelected.image = filePath;
            await _dbContext.SaveChangesAsync();
        }

        public async Task RemovePhotoFromRestaurant(Guid restaurantId)
        {
            var imageItem = await _dbContext.restaurantTable
                .Where(c => c.Id == restaurantId).FirstOrDefaultAsync();

            if (imageItem == null)
                throw new Exception("Фото не найдено.");

            if (File.Exists(imageItem.imagePath))
                File.Delete(imageItem.imagePath);

            imageItem.imagePath = string.Empty;
            await _dbContext.SaveChangesAsync();

        }

        public async Task RemovePhotoFromFoodItem(Guid fooditemId)
        {
            var imageItem = await _dbContext.restaurantFoodItemsTable
                .Where(c => c.Id == fooditemId).FirstOrDefaultAsync();

            if (imageItem == null)
                throw new Exception("Фото не найдено.");

            if (File.Exists(imageItem.image))
                File.Delete(imageItem.image);

            imageItem.image = string.Empty;
            await _dbContext.SaveChangesAsync();
        }    
    }
}
