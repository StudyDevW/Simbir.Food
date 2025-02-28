using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ORM_Components.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "basketTable",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    food_item_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_basketTable", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "courierTable",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    userId = table.Column<Guid>(type: "uuid", nullable: false),
                    car_number = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_courierTable", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "orderItemsTable",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    restaraunt_food_item = table.Column<int>(type: "integer", nullable: false),
                    price = table.Column<int>(type: "integer", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orderItemsTable", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "orderTable",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    client_id = table.Column<Guid>(type: "uuid", nullable: false),
                    restaurant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    courier_id = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    total_price = table.Column<int>(type: "integer", nullable: false),
                    order_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orderTable", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "restaurantFoodItemsTable",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    restaurant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    price = table.Column<int>(type: "integer", nullable: false),
                    image = table.Column<string>(type: "text", nullable: false),
                    weight = table.Column<int>(type: "integer", nullable: false),
                    calories = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_restaurantFoodItemsTable", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "restaurantTable",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    restaurantName = table.Column<string>(type: "text", nullable: false),
                    address = table.Column<string>(type: "text", nullable: false),
                    phone_number = table.Column<string>(type: "text", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    imagePath = table.Column<string>(type: "text", nullable: false),
                    open_time = table.Column<string>(type: "text", nullable: false),
                    close_time = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_restaurantTable", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "reviewTable",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    client_id = table.Column<Guid>(type: "uuid", nullable: false),
                    courier_id = table.Column<Guid>(type: "uuid", nullable: true),
                    restaurant_id = table.Column<Guid>(type: "uuid", nullable: true),
                    rating = table.Column<int>(type: "integer", nullable: false),
                    comment = table.Column<string>(type: "text", nullable: true),
                    review_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reviewTable", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "userTable",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    first_name = table.Column<string>(type: "text", nullable: false),
                    last_name = table.Column<string>(type: "text", nullable: true),
                    telegram_id = table.Column<long>(type: "bigint", nullable: false),
                    telegram_chat_id = table.Column<long>(type: "bigint", nullable: false),
                    address = table.Column<string>(type: "text", nullable: true),
                    photo_url = table.Column<string>(type: "text", nullable: true),
                    username = table.Column<string>(type: "text", nullable: true),
                    roles = table.Column<string[]>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_userTable", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "restaurantFoodItemsTable",
                columns: new[] { "Id", "calories", "image", "name", "price", "restaurant_id", "weight" },
                values: new object[,]
                {
                    { new Guid("5c34d1fa-46fc-4f5f-9ee2-94221e9f01df"), 2000, "NONE", "Тестовое блюдо", 1000, new Guid("54d33061-3691-4b7d-a60c-c53ef2e4eb4e"), 100 },
                    { new Guid("fef39909-1e0a-4360-95cc-e92fcf0324ce"), 1000, "NONE", "Тестовое блюдо 2", 1200, new Guid("54d33061-3691-4b7d-a60c-c53ef2e4eb4e"), 100 }
                });

            migrationBuilder.InsertData(
                table: "restaurantTable",
                columns: new[] { "Id", "address", "close_time", "description", "imagePath", "open_time", "phone_number", "restaurantName", "status", "user_id" },
                values: new object[,]
                {
                    { new Guid("54d33061-3691-4b7d-a60c-c53ef2e4eb4e"), "ул. Шолмова 5", "21:00", "Отличный тестовый ресторан", "NONE", "10:00", "+78005555535", "Тестовый ресторан", "approved", new Guid("1993856e-2f5c-4790-a3d4-33e6a5718b47") },
                    { new Guid("8108d64a-17b8-49a1-8ec5-3f9483735760"), "ул. Шолмова 3", "20:00", "Хороший тестовый ресторан", "NONE", "10:00", "+78004444434", "Тестовый ресторан 2", "approved", new Guid("1993856e-2f5c-4790-a3d4-33e6a5718b47") }
                });

            migrationBuilder.InsertData(
                table: "userTable",
                columns: new[] { "Id", "address", "first_name", "last_name", "photo_url", "roles", "telegram_chat_id", "telegram_id", "username" },
                values: new object[] { new Guid("1993856e-2f5c-4790-a3d4-33e6a5718b47"), "улица Шолмова, 7", "Антон (Study)", "", "https://t.me/i/userpic/320/YC895p02kbd-O-aU-F49vK8j1qFbmbObwS_DaaPkKdg.svg", new[] { "Client", "Admin", "Courier" }, 1006365928L, 1006365928L, "studywhite" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "basketTable");

            migrationBuilder.DropTable(
                name: "courierTable");

            migrationBuilder.DropTable(
                name: "orderItemsTable");

            migrationBuilder.DropTable(
                name: "orderTable");

            migrationBuilder.DropTable(
                name: "restaurantFoodItemsTable");

            migrationBuilder.DropTable(
                name: "restaurantTable");

            migrationBuilder.DropTable(
                name: "reviewTable");

            migrationBuilder.DropTable(
                name: "userTable");
        }
    }
}
