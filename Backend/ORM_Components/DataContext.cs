using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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

        public DataContext()
        {

        }
        public DataContext(string connectionString)
        {
            _connectionString = connectionString;
            Database.EnsureCreated();
        }

        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        public virtual DbSet<UserTable> userTable { get; set; }

        public virtual DbSet<CourierTable> courierTable { get; set; }

        public virtual DbSet<OrderItemsTable> orderItemsTable { get; set; }

        public virtual DbSet<OrderTable> orderTable { get; set; }

        public virtual DbSet<RestaurantTable> restaurantTable { get; set; }

        public virtual DbSet<RestaurantFoodItemsTable> restaurantFoodItemsTable { get; set; }

        public virtual DbSet<ReviewTable> reviewTable { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
                optionsBuilder.UseNpgsql(_connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
