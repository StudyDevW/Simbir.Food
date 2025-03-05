using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Middleware_Components.Services;
using ORM_Components;
using ORM_Components.DTO.RestaurantAPI;
using ORM_Components.Tables;
using RestaurantAPI.Model.Controllers.CustomAttributes;
using RestaurantAPI.Model.Interface;


namespace RestaurantAPI.Model.Controllers
{
    [Route("api/Photos")]
    [ApiController]
    public class PhotoController : ControllerBase
    {
        private readonly IPhotoServices _photoServices;

        public PhotoController(IPhotoServices photoServices)
        {
            _photoServices = photoServices;
        }

        [HttpPost("AddPhotoRestaurant")]
        public async Task<IActionResult> AddPhotoToRestaurant([FromForm] PhotoDTO_Restaurant photo_DTO)
        {
            try
            {
                await _photoServices.AddPhotoRestaurant(photo_DTO);
                return Ok("Фотография загружена");
            }
            catch (Exception ex) {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("AddPhotoFoodItem")]
        public async Task<IActionResult> AddPhotoToFoodItem([FromForm] PhotoDTO_FoodItem photo_DTO)
        {
            try
            {
                await _photoServices.AddPhotoRestaurantFoodItem(photo_DTO);
                return Ok("Фотография загружена");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("Image")]
        public IActionResult ImageOutput([FromHeader] string filePath)
        {
            try
            {
                var file = new FileStream(filePath, FileMode.Open, FileAccess.Read);

                return File(file, "image/jpeg");
            }
            catch (Exception e)
            {
                return BadRequest("image_not_found");
            }
        }

        [HttpDelete("RemovePhotoRestaurant/{id}")]
        public async Task<IActionResult> RemovePhotoRestaurant(Guid restaurantId)
        {
            await _photoServices.RemovePhotoFromRestaurant(restaurantId);
            return Ok();
        }

        [HttpDelete("RemovePhotoFoodItem/{id}")]
        public async Task<IActionResult> RemovePhotoFoodItem(Guid fooditemId)
        {
            await _photoServices.RemovePhotoFromFoodItem(fooditemId);
            return Ok();
        }
    }
}
