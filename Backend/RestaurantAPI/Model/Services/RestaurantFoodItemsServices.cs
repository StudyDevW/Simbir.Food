using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Middleware_Components.Services;
using ORM_Components;
using ORM_Components.DTO.RestaurantAPI;
using ORM_Components.Tables;
using RestaurantAPI.Model.Interface;

namespace RestaurantAPI.Model.Services
{
    public class RestaurantFoodItemsServices : IRestaurantFoodItemsServices
    {
        private readonly DataContext _dbcontext;
        private readonly IJwtService _jwtServices;

        public RestaurantFoodItemsServices(DataContext dbcontext, IJwtService jwtServices)
        {
            _dbcontext = dbcontext;
            _jwtServices = jwtServices;
        }

        public async Task<string> AddRestaurantFoodItems([FromBody] RestaurantFoodItems_DTO restaurantFoodItems_DTO)
        {
            if (restaurantFoodItems_DTO == null)
            {
                throw new Exception("Данные блюда не могут быть пустыми.");
            }

            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(restaurantFoodItems_DTO.name))
            {
                errors.Add("Название блюда не может быть пустым.");
            }
            if (restaurantFoodItems_DTO.price <= 0)
            {
                errors.Add("Цена блюда не может быть 0.");
            }
            if (restaurantFoodItems_DTO.weight <= 0)
            {
                errors.Add("Вес блюда не может быть 0.");
            }
            if (restaurantFoodItems_DTO.calories <= 0)
            {
                errors.Add("Калории блюда не могут быть 0.");
            }

            if (errors.Any())
            {
                throw new Exception(string.Join(",", errors.ToArray()));
            }

            var restaurantExists = await _dbcontext.restaurantTable.FindAsync(restaurantFoodItems_DTO.restaurant_id);
            if (restaurantExists == null)
            {
                throw new Exception("Ресторан с указанным ID не найден.");
            }

            var restaurantFoodItemsTable = new RestaurantFoodItemsTable()
            {
                restaurant_id = restaurantFoodItems_DTO.restaurant_id,
                name = restaurantFoodItems_DTO.name,
                price = restaurantFoodItems_DTO.price,
                image = restaurantFoodItems_DTO.image,
                weight = restaurantFoodItems_DTO.weight,
                calories = restaurantFoodItems_DTO.calories
            };

            _dbcontext.restaurantFoodItemsTable.Add(restaurantFoodItemsTable);
            await _dbcontext.SaveChangesAsync();

            return "Блюдо успешно добавлено.";
        }

        public async Task<string> DeleteRestaurantFoodItems(Guid id)
        {
            var restaurantFoodItems = await _dbcontext.restaurantFoodItemsTable.FindAsync(id);
            if (restaurantFoodItems == null)
            {
                throw new Exception("Блюдо не найдено.");
            }

            _dbcontext.restaurantFoodItemsTable.Remove(restaurantFoodItems);
            await _dbcontext.SaveChangesAsync();

            return "Блюдо успешно удалено.";
        }

        public async Task<string> DeleteAllRestaurantFoodItems()
        {
            var restaurantFoodItems = await _dbcontext.restaurantFoodItemsTable.ToListAsync();
            if (!restaurantFoodItems.Any())
            {
                throw new Exception("Нет доступных блюд для удаления.");
            }

            _dbcontext.restaurantFoodItemsTable.RemoveRange(restaurantFoodItems);
            await _dbcontext.SaveChangesAsync();

            return "Все блюда успешно удалены.";
        }

        public async Task<List<RestaurantFoodItemsTable>> GetRestaurantFoodItems(Guid restaurantId)
        {
            var restaurantFoodItems = await _dbcontext.restaurantFoodItemsTable
                .Where(c => c.restaurant_id == restaurantId)
                .ToListAsync();
            return restaurantFoodItems;
        }

        public async Task<List<RestaurantFoodItemsTable>> GetAllRestaurantFoodItems()
        {
            var restaurantFoodItems = await _dbcontext.restaurantFoodItemsTable.ToListAsync();
            return restaurantFoodItems;
        }

        public async Task<string> PutRestaurantFoodItems([FromBody] RestaurantFoodItems_DTO restaurantFoodItems_DTO, Guid food_Id)
        {
            if (restaurantFoodItems_DTO == null)
            {
                throw new Exception("Данные блюда не могут быть пустыми.");
            }

            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(restaurantFoodItems_DTO.name))
            {
                errors.Add("Название блюда не может быть пустым.");
            }
            if (restaurantFoodItems_DTO.price <= 0)
            {
                errors.Add("Цена блюда не может быть 0.");
            }
            if (restaurantFoodItems_DTO.weight <= 0)
            {
                errors.Add("Вес блюда не может быть 0.");
            }
            if (restaurantFoodItems_DTO.calories <= 0)
            {
                errors.Add("Калории блюда не могут быть 0.");
            }

            if (errors.Any())
            {
                throw new Exception(string.Join(",", errors.ToArray()));
            }

            var restaurantFoodItem = await _dbcontext.restaurantFoodItemsTable.FindAsync(food_Id);
            if (restaurantFoodItem == null)
            {
                throw new Exception("Блюдо с указанным ID не найдено.");
            }

            restaurantFoodItem.name = restaurantFoodItems_DTO.name;
            restaurantFoodItem.price = restaurantFoodItems_DTO.price;
            restaurantFoodItem.image = restaurantFoodItems_DTO.image;
            restaurantFoodItem.weight = restaurantFoodItems_DTO.weight;
            restaurantFoodItem.calories = restaurantFoodItems_DTO.calories;

            _dbcontext.restaurantFoodItemsTable.Update(restaurantFoodItem);
            await _dbcontext.SaveChangesAsync();

            return "Блюдо успешно обновлено.";
        }
    }
}
