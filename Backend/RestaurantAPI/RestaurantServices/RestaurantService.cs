using Microsoft.EntityFrameworkCore;
using ORM_Components;
using System;
using System.Data.SqlClient;
using System.Configuration;
using Microsoft.AspNetCore.Mvc;

namespace RestaurantAPI.Model.GetRastaurant
{
    public class RestaurantService: ControllerBase
    {
        private readonly DataContext _context;

        public RestaurantService(DataContext context)
        {
            _context = context;
        }

        public async Task<string> GetRestaurantNameByIdAsync(Guid restaurantId)
        {
            var restaurant = await _context.restaurantTable
                .FirstOrDefaultAsync(r => r.Id == restaurantId);

            return restaurant?.restaurantName; 
        
        }
    }
}
