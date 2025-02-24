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
        private readonly IPhotoServices _photoServices;
        private readonly DataContext _dbContext;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IJwtService _jwtServices;

        public PhotoServices(DataContext dbContext, IWebHostEnvironment hostingEnvironment, IJwtService jwtServices, IPhotoServices photoServices)
        {
            _jwtServices = jwtServices;
            _dbContext = dbContext;
            _hostingEnvironment = hostingEnvironment;
            _photoServices = photoServices;
        }
        public async Task<IPhotoServices> AddPhotos([FromForm] Photos_DTO photo_DTO)
        {       
            if (photo_DTO.File == null || photo_DTO.File.Length == 0)
            {
                throw new Exception("Файл не был загружен.");
            }

            var filePath = Path.Combine(_hostingEnvironment.WebRootPath, "uploads", photo_DTO.File.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await photo_DTO.File.CopyToAsync(stream);
            }

            var restaurantItem = new RestaurantTable()
            {
                imagePath = $"/uploads/{photo_DTO.File.FileName}", // Путь к файлу
            };

            _dbContext.restaurantTable.Add(restaurantItem);
            await _dbContext.SaveChangesAsync();
            throw new Exception("Файл был успешно загружен  .");

        }
        public async Task<IPhotoServices> RemovePhoto(int id)
        {
            var restaurantItem = await _dbContext.restaurantTable.FindAsync(id);

            if (restaurantItem == null)
            {
                throw new Exception("Фото не найдено.");
                return null;
            }

            _dbContext.restaurantTable.Remove(restaurantItem);
            await _dbContext.SaveChangesAsync();

            throw new Exception("Фото успешно удалено.");
        }
        public async Task<IPhotoServices> RemoveAllPhotos()
        {
            var restaurantItems = await _dbContext.restaurantTable.ToListAsync();

            if (restaurantItems == null || !restaurantItems.Any())
            {
                throw new Exception("Нет доступных фотографий для удаления.");
            }

            _dbContext.restaurantTable.RemoveRange(restaurantItems);
            await _dbContext.SaveChangesAsync();

            throw new Exception("Все фотографии успешно удалены.");
        }

        public async Task<List<RestaurantTable>> GetPhotos()
        {
            var restaurantItems = await _dbContext.restaurantTable.ToListAsync();
            
            return(restaurantItems);
        }

        public async Task<List<RestaurantTable>> GetAllPhotos(Guid restaurantId)
        {

            var restaurantAllItems = await _dbContext.restaurantTable.Where(c => c.user_id == restaurantId).ToListAsync();
            return(restaurantAllItems);
        }
    }
}
