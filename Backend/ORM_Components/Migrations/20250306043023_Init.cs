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
                name: "bankCardTable",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    card_number = table.Column<string>(type: "text", nullable: false),
                    cvv = table.Column<string>(type: "text", nullable: false),
                    money_value = table.Column<long>(type: "bigint", nullable: false),
                    name_card = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bankCardTable", x => x.Id);
                });

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
                name: "cardUsersTable",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    card_number = table.Column<string>(type: "text", nullable: false),
                    cvv = table.Column<string>(type: "text", nullable: false),
                    money_value = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cardUsersTable", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "courierTable",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    userId = table.Column<Guid>(type: "uuid", nullable: false),
                    car_number = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_courierTable", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "orderHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    status_datetime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orderHistory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "orderItemsTable",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    restaraunt_food_item = table.Column<Guid>(type: "uuid", nullable: false)
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
                    total_price = table.Column<long>(type: "bigint", nullable: false),
                    order_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orderTable", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "payTable",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    pay_status = table.Column<int>(type: "integer", nullable: false),
                    date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    card_number = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payTable", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "requestTable",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    restaurant_id = table.Column<Guid>(type: "uuid", nullable: true),
                    courier_id = table.Column<Guid>(type: "uuid", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    time_add = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_requestTable", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "restaurantFoodItemsTable",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    restaurant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    price = table.Column<long>(type: "bigint", nullable: false),
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
                    status = table.Column<int>(type: "integer", nullable: false),
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
                    money_value = table.Column<long>(type: "bigint", nullable: false),
                    email = table.Column<string>(type: "text", nullable: true),
                    roles = table.Column<string[]>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_userTable", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "bankCardTable",
                columns: new[] { "Id", "card_number", "cvv", "money_value", "name_card" },
                values: new object[] { new Guid("e11ba92a-649b-4ea7-8881-c4d7840be3a0"), "0000 1234 0000 4321", "123", 1000000L, "Virtual Card" });

            migrationBuilder.InsertData(
                table: "restaurantFoodItemsTable",
                columns: new[] { "Id", "calories", "image", "name", "price", "restaurant_id", "weight" },
                values: new object[,]
                {
                    { new Guid("0dcfbe90-6964-4064-af66-280920c81943"), 1000, "NONE", "Тестовое блюдо 2", 1200L, new Guid("54d33061-3691-4b7d-a60c-c53ef2e4eb4e"), 100 },
                    { new Guid("4e37119d-878c-41c4-b632-9e87e99b45ab"), 2000, "NONE", "Тестовое блюдо", 1000L, new Guid("54d33061-3691-4b7d-a60c-c53ef2e4eb4e"), 100 }
                });

            migrationBuilder.InsertData(
                table: "restaurantTable",
                columns: new[] { "Id", "address", "close_time", "description", "imagePath", "open_time", "phone_number", "restaurantName", "status", "user_id" },
                values: new object[,]
                {
                    { new Guid("54d33061-3691-4b7d-a60c-c53ef2e4eb4e"), "ул. Шолмова 5", "21:00", "Отличный тестовый ресторан", "NONE", "10:00", "+78005555535", "Тестовый ресторан", 1, new Guid("1993856e-2f5c-4790-a3d4-33e6a5718b47") },
                    { new Guid("e25ca427-7d35-49d3-8793-ce9a2975c53a"), "ул. Шолмова 3", "20:00", "Хороший тестовый ресторан", "NONE", "10:00", "+78004444434", "Тестовый ресторан 2", 1, new Guid("1993856e-2f5c-4790-a3d4-33e6a5718b47") }
                });

            migrationBuilder.InsertData(
                table: "userTable",
                columns: new[] { "Id", "address", "email", "first_name", "last_name", "money_value", "photo_url", "roles", "telegram_chat_id", "telegram_id", "username" },
                values: new object[] { new Guid("1993856e-2f5c-4790-a3d4-33e6a5718b47"), "улица Шолмова, 7", "", "Антон (Study)", "", 5000L, "https://t.me/i/userpic/320/YC895p02kbd-O-aU-F49vK8j1qFbmbObwS_DaaPkKdg.svg", new[] { "Client", "Admin", "Courier" }, 1006365928L, 1006365928L, "studywhite" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bankCardTable");

            migrationBuilder.DropTable(
                name: "basketTable");

            migrationBuilder.DropTable(
                name: "cardUsersTable");

            migrationBuilder.DropTable(
                name: "courierTable");

            migrationBuilder.DropTable(
                name: "orderHistory");

            migrationBuilder.DropTable(
                name: "orderItemsTable");

            migrationBuilder.DropTable(
                name: "orderTable");

            migrationBuilder.DropTable(
                name: "payTable");

            migrationBuilder.DropTable(
                name: "requestTable");

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
