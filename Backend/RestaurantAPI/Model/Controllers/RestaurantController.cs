using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ORM_Components;
using ORM_Components.DTO.ClientAPI;
using ORM_Components.DTO.RestaurantAPI;
using ORM_Components.Tables;
using RestaurantAPI.Model.DBO;

namespace RestaurantAPI.Model.Controllers
{
    [Route("api/Restaurant/")]
    [ApiController]
    public class RestaurantController : ControllerBase
    {
        private readonly DataContext _dbcontext;
        public RestaurantController(DataContext dbcontext)
        {
            
            _dbcontext = dbcontext;
        }
        [HttpPost]
        [Route("SentRestaurant")]
        public async Task<IActionResult> AddRestaurant([FromBody] Restaurant restaurant_DTO)
        {   
            RestaurantTable restaurantTable = new RestaurantTable()
            {
                id = restaurant_DTO.id,
                name = restaurant_DTO.name,
                Img = restaurant_DTO.Img,
            };
            _dbcontext.restaurantTable.Add(restaurantTable);
            await _dbcontext.SaveChangesAsync();
            return Ok("Успех");

            
        }
        
        [HttpDelete]
        [Route("DeleteRestautant/{id}")]
        public async Task<IActionResult> DeleteRestautant(int id)
        {
            using (DataContext DB = new DataContext())
            {
                var Restautant = await DB.restaurantTable.FindAsync(id);
                if (Restautant == null) {
                    return NotFound("Ресторан не найден.");
                }
                DB.restaurantTable.Remove(Restautant);
                await DB.SaveChangesAsync();
                return Ok("Ресторан успешно удалён");

            }
        }
        [HttpGet]
        [Route("GetRestaurant")]
        public async Task<IActionResult> GetRestaurant()
        {
            using (DataContext DB = new DataContext())
            {
                var Restautant = await DB.restaurantTable.ToListAsync();
                return Ok(Restautant);
            }
        }
    }
}
