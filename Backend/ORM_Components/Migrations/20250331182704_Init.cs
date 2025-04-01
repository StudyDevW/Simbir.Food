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
                    courier_id = table.Column<Guid>(type: "uuid", nullable: false),
                    restaurant_id = table.Column<Guid>(type: "uuid", nullable: false),
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
                    { new Guid("0983151c-9ba3-4900-b494-07e99a10cb0c"), 0, "/app/migrated_images/Vkusno/vishnyapirojok.png", "Пирожок Вишневый", 84L, new Guid("54d33061-3691-4b7d-a60c-c53ef2e4eb4e"), 0 },
                    { new Guid("131ad21e-2c19-4bce-afc8-d88b56c1094d"), 0, "/app/migrated_images/Vkusno/chikenpremiere.png", "Чикен Премьер", 214L, new Guid("54d33061-3691-4b7d-a60c-c53ef2e4eb4e"), 0 },
                    { new Guid("1cf13caa-6884-4a8d-8a01-5a4caea2619c"), 0, "/app/migrated_images/Vkusno/cezarroll.png", "Цезарь Ролл", 242L, new Guid("54d33061-3691-4b7d-a60c-c53ef2e4eb4e"), 0 },
                    { new Guid("1da2b42e-fac0-4c01-98ad-a0959f1b5d96"), 0, "/app/migrated_images/Vkusno/cheezeburger.png", "Чизбургер", 101L, new Guid("54d33061-3691-4b7d-a60c-c53ef2e4eb4e"), 0 },
                    { new Guid("23e7dfb3-a218-4329-b7b8-588179e6f1d9"), 0, "/app/migrated_images/Vkusno/bighit.png", "Биг Хит", 223L, new Guid("54d33061-3691-4b7d-a60c-c53ef2e4eb4e"), 0 },
                    { new Guid("4b52d1e2-4029-460e-bfb2-011b474640ae"), 0, "/app/migrated_images/Vkusno/potatofreesred.png", "Картофель Фри средний", 112L, new Guid("54d33061-3691-4b7d-a60c-c53ef2e4eb4e"), 0 },
                    { new Guid("65136a23-c7dd-48ae-b17f-a6feb4706924"), 0, "/app/migrated_images/Vkusno/naggets6.png", "Наггетсы (6 шт)", 109L, new Guid("54d33061-3691-4b7d-a60c-c53ef2e4eb4e"), 0 },
                    { new Guid("688113e8-cef7-4b41-b9d6-1b237efef01d"), 0, "/app/migrated_images/Vkusno/doublecheezeburger.png", "Двойной Чизбургер", 195L, new Guid("54d33061-3691-4b7d-a60c-c53ef2e4eb4e"), 0 },
                    { new Guid("81b65cf4-29ff-4110-9d10-1a3753af4593"), 0, "/app/migrated_images/Vkusno/bigspecial.png", "Биг спешиал", 352L, new Guid("54d33061-3691-4b7d-a60c-c53ef2e4eb4e"), 0 },
                    { new Guid("984a10cc-b58a-40fb-bc30-6ffa96dbb45d"), 0, "/app/migrated_images/Vkusno/capuchinosred.png", "Капучино (сред.)", 134L, new Guid("54d33061-3691-4b7d-a60c-c53ef2e4eb4e"), 0 },
                    { new Guid("df61599f-0933-49b8-9735-be616bb4e4a6"), 0, "/app/migrated_images/Vkusno/chikenburger.png", "Чикенбургер", 82L, new Guid("54d33061-3691-4b7d-a60c-c53ef2e4eb4e"), 0 }
                });

            migrationBuilder.InsertData(
                table: "restaurantTable",
                columns: new[] { "Id", "address", "close_time", "description", "imagePath", "open_time", "phone_number", "restaurantName", "status", "user_id" },
                values: new object[,]
                {
                    { new Guid("1d55fc0c-6f29-4980-9855-4a3397d1be20"), "ул. Шолмова 3", "20:00", "Хороший тестовый ресторан", "/app/migrated_images/burgerking.jpg", "10:00", "+78004444434", "Бургер Кинг", 1, new Guid("1993856e-2f5c-4790-a3d4-33e6a5718b47") },
                    { new Guid("54d33061-3691-4b7d-a60c-c53ef2e4eb4e"), "ул. Шолмова 5", "21:00", "Отличный тестовый ресторан", "/app/migrated_images/vkusno.jpg", "10:00", "+78005555535", "Вкусно и точка", 1, new Guid("1993856e-2f5c-4790-a3d4-33e6a5718b47") }
                });

            migrationBuilder.InsertData(
                table: "reviewTable",
                columns: new[] { "Id", "client_id", "comment", "courier_id", "order_id", "rating", "restaurant_id", "review_date" },
                values: new object[,]
                {
                    { new Guid("9ae9cea3-d065-4c5f-84bf-3485da8295d6"), new Guid("1993856e-2f5c-4790-a3d4-33e6a5718b47"), null, new Guid("729d9e64-faf1-439b-95f9-5a29a0e3e969"), new Guid("871c8f7f-c4b1-46b3-b959-a6ccfbee534b"), 5, new Guid("54d33061-3691-4b7d-a60c-c53ef2e4eb4e"), new DateTime(2025, 3, 31, 18, 27, 4, 335, DateTimeKind.Utc).AddTicks(3064) },
                    { new Guid("acc48a14-1e54-49df-89d7-54be073758ce"), new Guid("1993856e-2f5c-4790-a3d4-33e6a5718b47"), null, new Guid("bbcad18c-4e90-4541-9b33-d661cec2fc5a"), new Guid("dd932e2a-972b-4517-9c64-30e48b0bd052"), 4, new Guid("54d33061-3691-4b7d-a60c-c53ef2e4eb4e"), new DateTime(2025, 3, 31, 18, 27, 4, 335, DateTimeKind.Utc).AddTicks(3069) }
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
