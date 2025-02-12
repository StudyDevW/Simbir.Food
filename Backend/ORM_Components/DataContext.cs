using Microsoft.EntityFrameworkCore;
using ORM_Components.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ORM_Components
{
    public class DataContext : DbContext
    {
        private readonly string _connectionString;

        public DataContext(string connectionString)
        {
            _connectionString = connectionString;
            Database.EnsureCreated();
        }

        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public DbSet<UserTable> userTable { get; set; }

        public DbSet<CourierTable> courierTable { get; set; }

        public DbSet<OrderItemsTable> orderItemsTable { get; set; }

        public DbSet<OrderTable> orderTable { get; set; }

        public DbSet<RestaurantTable> restaurantTable { get; set; }

        public DbSet<RestaurantFoodItemsTable> restaurantFoodItemsTable { get; set; }

        public DbSet<ReviewTable> reviewTable { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured && _connectionString != null)
                optionsBuilder.UseNpgsql(_connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
