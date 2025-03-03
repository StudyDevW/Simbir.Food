
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
    [Route("api/Photos")]
    //[ValidateJwt]
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
        public async Task<ActionResult> AddPhoto([FromForm] Photos_DTO photo_DTO)
        {
            try
            {
                await _photoServices.AddPhotos(photo_DTO);
                return Ok("Фотография загружена");
            }
            catch (Exception ex) {
                return BadRequest(ex.Message);
            }
        }
        [HttpDelete("RemovePhoto/{id}")]
        public async Task<ActionResult> RemovePhoto(int id)
        {
            await _photoServices.RemovePhoto(id);
            return NoContent();
        }
        [HttpDelete("RemoveRangePhoto/{id}")]
        public async Task<ActionResult> RemoveRangePhoto(int id)
        {
            await _photoServices.RemoveAllPhotos();
            return NoContent();
        }
        [HttpGet("GetPhotos/{id}")]
        public async Task<List<RestaurantTable>> GetPhotos()
        {
             return await _photoServices.GetPhotos();
        }
        [HttpGet("GetAllPhotos")]
        public async Task<List<RestaurantTable>> GetAllPhotos(Guid restaurantId) 
        {
            return await _photoServices.GetAllPhotos(restaurantId);
        }
    }
}
