using FluentValidation;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Middleware_Components.Services;
using ORM_Components;
using ORM_Components.DTO.CourierAPI;
using ORM_Components.DTO.RestaurantAPI;
using ORM_Components.Tables;
using RestaurantAPI.Model.Interface;

namespace RestaurantAPI.Model.Services
{
    public class RestaurantFoodItemsServices : IRestaurantFoodItemsServices
    {
        private readonly DataContext _dbcontext;
        private readonly IJwtService _jwtServices;
        private readonly IValidator<RestaurantFoodItemsDtoForCreate> _validatorForCreateFoodItem;
        private readonly IValidator<RestaurantFoodItemsDtoForUpdate> _validatorForUpdateFoodItem;

        public RestaurantFoodItemsServices(DataContext dbcontext, IJwtService jwtServices,
            IValidator<RestaurantFoodItemsDtoForCreate> validatorForCreateFoodItem, IValidator<RestaurantFoodItemsDtoForUpdate> validatorForUpdateFoodItem)
        {
            _dbcontext = dbcontext;
            _jwtServices = jwtServices;
            _validatorForCreateFoodItem = validatorForCreateFoodItem;
            _validatorForUpdateFoodItem = validatorForUpdateFoodItem;
        }

        public async Task AddRestaurantFoodItems(RestaurantFoodItemsDtoForCreate restaurantFoodItemsDtoForCreate)
        {
            await _validatorForCreateFoodItem.ValidateAndThrowAsync(restaurantFoodItemsDtoForCreate);

            var restaurantExists = await _dbcontext.restaurantTable.FirstOrDefaultAsync(x => x.Id == restaurantFoodItemsDtoForCreate.restaurant_id);
            if (restaurantExists == null)
            {
                throw new Exception("Ресторан с указанным ID не найден.");
            }

            var foodItem = restaurantFoodItemsDtoForCreate.Adapt<RestaurantFoodItemsTable>();

            _dbcontext.restaurantFoodItemsTable.Add(foodItem);
            await _dbcontext.SaveChangesAsync();
        }

        public async Task DeleteRestaurantFoodItems(Guid id)
        {
            var restaurantFoodItems = await _dbcontext.restaurantFoodItemsTable.FindAsync(id);
            if (restaurantFoodItems == null)
            {
                throw new Exception("Блюдо не найдено.");
            }

            _dbcontext.restaurantFoodItemsTable.Remove(restaurantFoodItems);
            await _dbcontext.SaveChangesAsync();
        }

        public async Task DeleteAllRestaurantFoodItems(Guid restaurantId)
        {
            var restaurantFoodItems = await _dbcontext.restaurantFoodItemsTable
                .Where(item => item.restaurant_id == restaurantId)
                .ToListAsync();

            if (!restaurantFoodItems.Any())
            {
                throw new Exception("Нет доступных блюд для удаления в указанном ресторане.");
            }

            _dbcontext.restaurantFoodItemsTable.RemoveRange(restaurantFoodItems);
            await _dbcontext.SaveChangesAsync();
        }


        public async Task<List<RestaurantFoodItemsDto>> GetRestaurantFoodItems(Guid restaurantId)
        {
            var restaurantFoodItems = await _dbcontext.restaurantFoodItemsTable
                .Where(c => c.restaurant_id == restaurantId)
                .Select(x => new RestaurantFoodItemsDto(
                    x.Id, x.restaurant_id,
                    x.name, x.price,
                    x.image, x.weight, 
                    x.calories))
                .ToListAsync();
            return restaurantFoodItems;
        }

        public async Task<List<RestaurantFoodItemsDto>> GetAllRestaurantFoodItems()
        {
            var restaurantFoodItems = await _dbcontext.restaurantFoodItemsTable
                .Select(x => new RestaurantFoodItemsDto(
                    x.Id, x.restaurant_id,
                    x.name, x.price,
                    x.image, x.weight,
                    x.calories))
                .ToListAsync();
            return restaurantFoodItems;
        }

        public async Task UpdateRestaurantFoodItems(Guid food_Id, RestaurantFoodItemsDtoForUpdate restaurantFoodItemsDtoForUpdate)
        {
            await _validatorForUpdateFoodItem.ValidateAndThrowAsync(restaurantFoodItemsDtoForUpdate);

            var restaurantFoodItem = await _dbcontext.restaurantFoodItemsTable.FirstOrDefaultAsync(x=> x.Id == food_Id);
            if (restaurantFoodItem == null)
            {
                throw new Exception("Блюдо с указанным ID не найдено.");
            }

            var config = new TypeAdapterConfig();
            config.Default.IgnoreNullValues(true);
            restaurantFoodItemsDtoForUpdate.Adapt(restaurantFoodItem, config);
            await _dbcontext.SaveChangesAsync();
        }
    }
}
