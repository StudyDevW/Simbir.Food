using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ORM_Components.Tables;
using ORM_Components.Tables.Helpers;
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

        public DbSet<OrderStatusHistoryTable> orderHistory { get; set; }

        public DbSet<RestaurantTable> restaurantTable { get; set; }

        public DbSet<RestaurantFoodItemsTable> restaurantFoodItemsTable { get; set; }

        public DbSet<ReviewTable> reviewTable { get; set; }

        public DbSet<BasketTable> basketTable { get; set; }

        public DbSet<CardUsersTable> cardUsersTable { get; set; } 

        public DbSet<BankCardTable> bankCardTable { get; set; }

        public DbSet<PayTable> payTable { get; set; }

        public DbSet<RequestTable> requestTable { get; set; }

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
                    restaurantName = "Вкусно и точка",
                    address = "ул. Шолмова 5",
                    close_time = "21:00",
                    open_time = "10:00",
                    phone_number = "+78005555535",
                    description = "Отличный тестовый ресторан",
                    imagePath = "/app/migrated_images/vkusno.jpg",
                    status = RestaurantStatus.Verified
                },
                new RestaurantTable()
                {
                    Id = Guid.NewGuid(),
                    user_id = Guid.Parse("1993856e-2f5c-4790-a3d4-33e6a5718b47"),
                    restaurantName = "Бургер Кинг",
                    address = "ул. Шолмова 3",
                    close_time = "20:00",
                    open_time = "10:00",
                    phone_number = "+78004444434",
                    description = "Хороший тестовый ресторан",
                    imagePath = "/app/migrated_images/burgerking.jpg",
                    status = RestaurantStatus.Verified
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
                    money_value = 5000,
                    roles = new[] { "Client", "Admin", "Courier" }
                }
            );

            //Виртуальная карта
            modelBuilder.Entity<BankCardTable>().HasData(
                new BankCardTable()
                {
                    Id = Guid.Parse("e11ba92a-649b-4ea7-8881-c4d7840be3a0"),
                    card_number = "0000 1234 0000 4321",
                    cvv = "123",
                    money_value = 1000000,
                    name_card = "Virtual Card"
                }
            );

            base.OnModelCreating(modelBuilder);
        }
    }
}
