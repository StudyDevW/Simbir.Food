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
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IJwtService _jwtServices;

        public PhotoServices(DataContext dbContext, IWebHostEnvironment hostingEnvironment, IJwtService jwtServices)
        {
            _jwtServices = jwtServices;
            _dbContext = dbContext;
            _hostingEnvironment = hostingEnvironment;
        }
        public async Task ValidationTokenPhoto([FromForm] Photos_DTO photo_DTO, string token)
        {
            var validation = await _jwtServices.AccessTokenValidation(token);

            if (validation.TokenHasError())
            {
                throw new Exception("Unauthorized");
            }
            else if (validation.TokenHasSuccess())
            {
                if (photo_DTO.File == null || photo_DTO.File.Length == 0)
                {
                    throw new Exception("BadRequestv");
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
                return;
            }
            return;
        }
    }
}