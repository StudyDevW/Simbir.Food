using Microsoft.EntityFrameworkCore;
using ORM_Components;
using ORM_Components.DTO.RestaurantAPI;
using RestaurantAPI.Interface;
using RestaurantAPI.Model.Interface;

namespace RestaurantAPI.Model.Services
{
    public class PhotoServices : IPhotoServices
    {
        private readonly DataContext _dbContext;
        private readonly IFileSystemHandler _fileSystem;

        public PhotoServices(DataContext dbContext, IFileSystemHandler fileSystem)
        {
            _dbContext = dbContext;
            _fileSystem = fileSystem;
        }

        public async Task AddPhotoRestaurant(PhotoDTO_Restaurant photo_DTO)
        {
            var restaurantSelected = _dbContext.restaurantTable
                .Where(c => c.Id == photo_DTO.restaurantId).FirstOrDefault();

            if (restaurantSelected == null)
                throw new Exception("restaurant_not_found");

            var filePath = await _fileSystem.AddPhoto(photo_DTO.File);

            restaurantSelected.imagePath = filePath;
            await _dbContext.SaveChangesAsync();
        }

        public async Task AddPhotoRestaurantFoodItem(PhotoDTO_FoodItem photo_DTO)
        {
            var foodItemSelected = _dbContext.restaurantFoodItemsTable
                .Where(c => c.Id == photo_DTO.fooditemId).FirstOrDefault();

            if (foodItemSelected == null)
                throw new Exception("fooditem_not_found");

            var filePath = await _fileSystem.AddPhoto(photo_DTO.File);

            foodItemSelected.image = filePath;
            await _dbContext.SaveChangesAsync();
        }

        public async Task RemovePhotoFromRestaurant(Guid restaurantId)
        {
            var imageItem = await _dbContext.restaurantTable
                .Where(c => c.Id == restaurantId).FirstOrDefaultAsync();

            if (imageItem == null)
                throw new Exception("Фото не найдено.");

            _fileSystem.DeletePhoto(imageItem.imagePath);

            imageItem.imagePath = string.Empty;
            await _dbContext.SaveChangesAsync();

        }

        public async Task RemovePhotoFromFoodItem(Guid fooditemId)
        {
            var imageItem = await _dbContext.restaurantFoodItemsTable
                .Where(c => c.Id == fooditemId).FirstOrDefaultAsync();

            if (imageItem == null)
                throw new Exception("Фото не найдено.");

            _fileSystem.DeletePhoto(imageItem.image);

            imageItem.image = string.Empty;
            await _dbContext.SaveChangesAsync();
        }    
    }
}
