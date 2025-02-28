using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Middleware_Components.Services;
using ORM_Components;
using ORM_Components.DTO.RestaurantAPI;
using ORM_Components.Tables;
using RestaurantAPI.Model.Interface;


namespace RestaurantAPI.Model.Controllers
{
    [Authorize(AuthenticationSchemes = "Asymmetric")]
    [Route("api/Photos")]
    [ApiController]
    public class PhotoController : ControllerBase
    {
        private readonly IPhotoServices _photoServices;
        private readonly DataContext _dbContext;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IJwtService _jwtServices;

        public PhotoController(DataContext dbContext, IWebHostEnvironment hostingEnvironment, IJwtService jwtServices, IPhotoServices photoServices)
        {
            _jwtServices = jwtServices;
            _dbContext = dbContext;
            _hostingEnvironment = hostingEnvironment;
            _photoServices = photoServices;
        }

        [HttpPost("AddPhoto")]
        public async Task<IActionResult> AddPhoto([FromForm] Photos_DTO photo_DTO)
        {
            //var photoServices = await _photoServices.;
            var validation = await _jwtServices.AccessTokenValidation(Request.Headers["Authorization"]);

            if (validation.TokenHasError())
            {
                return Unauthorized();
            }
            else if (validation.TokenHasSuccess())
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
            return BadRequest();
        }

        [HttpDelete("RemovePhoto/{id}")]
        public async Task<IActionResult> RemovePhoto(int id)
        {
            var validation = await _jwtServices.AccessTokenValidation(Request.Headers["Authorization"]);

            if (validation.TokenHasError())
            {
                return Unauthorized();
            }
            else if (validation.TokenHasSuccess())
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
            return BadRequest();
        }

        [HttpGet("GetPhotos")]
        public async Task<IActionResult> GetPhotos()
        {
            var validation = await _jwtServices.AccessTokenValidation(Request.Headers["Authorization"]);

            if (validation.TokenHasError())
            {
                return Unauthorized();
            }
            else if (validation.TokenHasSuccess())
            {
                var restaurantItems = await _dbContext.restaurantTable.ToListAsync();
                return Ok(restaurantItems);
            }
            return BadRequest();
            
        }
    }
}
