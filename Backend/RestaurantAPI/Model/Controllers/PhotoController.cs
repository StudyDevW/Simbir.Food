using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ORM_Components;
using ORM_Components.DTO.RestaurantAPI;
using ORM_Components.Tables;
using RestaurantAPI.Model.DBO;

namespace RestaurantAPI.Model.Controllers
{
    [Route("api/Photos")]
    [ApiController]
    public class PhotoController : ControllerBase
    {
        private readonly DataContext _dbContext;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public PhotoController(DataContext dbContext, IWebHostEnvironment hostingEnvironment)
        {
            _dbContext = dbContext;
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpPost("AddPhoto")]
        public async Task<IActionResult> AddPhoto([FromForm] Photos_DTO photo_DTO)
        {
            if (photo_DTO.File == null || photo_DTO.File.Length == 0)
            {
                return BadRequest("Файл не был загружен.");
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

            return Ok("Фото успешно добавлено.");
        }

        [HttpDelete("RemovePhoto/{id}")]
        public async Task<IActionResult> RemovePhoto(int id)
        {
            var restaurantItem = await _dbContext.restaurantTable.FindAsync(id);
            if (restaurantItem == null)
            {
                return NotFound("Фото не найдено.");
            }

            _dbContext.restaurantTable.Remove(restaurantItem);
            await _dbContext.SaveChangesAsync();

            return Ok("Фото успешно удалено.");
        }

        [HttpGet("GetPhotos")]
        public async Task<IActionResult> GetPhotos()
        {
            var restaurantItems = await _dbContext.restaurantTable.ToListAsync();
            return Ok(restaurantItems);
        }
    }
}
