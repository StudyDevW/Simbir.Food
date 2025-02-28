using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ORM_Components.Tables;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
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

        public DbSet<BasketTable> basketTable { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
                optionsBuilder.UseNpgsql(_connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RestaurantTable>().HasData(
                new RestaurantTable() {
                    Id = Guid.Parse("54d33061-3691-4b7d-a60c-c53ef2e4eb4e"),
                    user_id = Guid.Parse("1993856e-2f5c-4790-a3d4-33e6a5718b47"),
                    restaurantName = "Тестовый ресторан",
                    address = "ул. Шолмова 5",
                    close_time = "21:00",
                    open_time = "10:00",
                    phone_number = "+78005555535",
                    description = "Отличный тестовый ресторан",
                    imagePath = "NONE",
                    status = "approved"
                },
                new RestaurantTable()
                {
                    Id = Guid.NewGuid(),
                    user_id = Guid.Parse("1993856e-2f5c-4790-a3d4-33e6a5718b47"),
                    restaurantName = "Тестовый ресторан 2",
                    address = "ул. Шолмова 3",
                    close_time = "20:00",
                    open_time = "10:00",
                    phone_number = "+78004444434",
                    description = "Хороший тестовый ресторан",
                    imagePath = "NONE",
                    status = "approved"
                }
            );

            modelBuilder.Entity<RestaurantFoodItemsTable>().HasData(
                new RestaurantFoodItemsTable()
                {
                    Id = Guid.NewGuid(),
                    restaurant_id = Guid.Parse("54d33061-3691-4b7d-a60c-c53ef2e4eb4e"),
                    name = "Тестовое блюдо",
                    calories = 2000,
                    image = "NONE",
                    price = 1000,
                    weight = 100
                },
                new RestaurantFoodItemsTable()
                {
                    Id = Guid.NewGuid(),
                    restaurant_id = Guid.Parse("54d33061-3691-4b7d-a60c-c53ef2e4eb4e"),
                    name = "Тестовое блюдо 2",
                    calories = 1000,
                    image = "NONE",
                    price = 1200,
                    weight = 100
                }
            );

            //Тут можно свой профиль добавить
            modelBuilder.Entity<UserTable>().HasData(
                new UserTable()
                {
                    Id = Guid.Parse("1993856e-2f5c-4790-a3d4-33e6a5718b47"),
                    first_name = "Антон (Study)",
                    last_name = "",
                    telegram_id = 1006365928,
                    telegram_chat_id = 1006365928,
                    address = "улица Шолмова, 7",
                    photo_url = "https://t.me/i/userpic/320/YC895p02kbd-O-aU-F49vK8j1qFbmbObwS_DaaPkKdg.svg",
                    username = "studywhite",
                    roles = new[] { "Client", "Admin", "Courier" }
                }
            );

      

            base.OnModelCreating(modelBuilder);
        }
    }
}
