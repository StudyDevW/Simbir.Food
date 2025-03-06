using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ORM_Components.Migrations
{
    public partial class AddedEmailField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "restaurantFoodItemsTable",
                keyColumn: "Id",
                keyValue: new Guid("2fd96345-1feb-47b9-9db1-cd253c43334f"));

            migrationBuilder.DeleteData(
                table: "restaurantFoodItemsTable",
                keyColumn: "Id",
                keyValue: new Guid("977a4622-7940-4269-8b72-c02892702b3f"));

            migrationBuilder.DeleteData(
                table: "restaurantTable",
                keyColumn: "Id",
                keyValue: new Guid("fda6f2af-8dc4-4db3-95d5-8bf47130c92b"));

            migrationBuilder.AddColumn<string>(
                name: "email",
                table: "userTable",
                type: "text",
                nullable: true);

            migrationBuilder.InsertData(
                table: "restaurantFoodItemsTable",
                columns: new[] { "Id", "calories", "image", "name", "price", "restaurant_id", "weight" },
                values: new object[,]
                {
                    { new Guid("7a4154a5-9e58-4df8-ae1a-bc02898f47ec"), 2000, "NONE", "Тестовое блюдо", 1000L, new Guid("54d33061-3691-4b7d-a60c-c53ef2e4eb4e"), 100 },
                    { new Guid("d1426c2f-81d1-430c-9701-6542b246fdc9"), 1000, "NONE", "Тестовое блюдо 2", 1200L, new Guid("54d33061-3691-4b7d-a60c-c53ef2e4eb4e"), 100 }
                });

            migrationBuilder.InsertData(
                table: "restaurantTable",
                columns: new[] { "Id", "address", "close_time", "description", "imagePath", "open_time", "phone_number", "restaurantName", "status", "user_id" },
                values: new object[] { new Guid("800a1f58-30f1-4b14-a612-aaf987bdc7fa"), "ул. Шолмова 3", "20:00", "Хороший тестовый ресторан", "NONE", "10:00", "+78004444434", "Тестовый ресторан 2", 1, new Guid("1993856e-2f5c-4790-a3d4-33e6a5718b47") });

            migrationBuilder.UpdateData(
                table: "userTable",
                keyColumn: "Id",
                keyValue: new Guid("1993856e-2f5c-4790-a3d4-33e6a5718b47"),
                column: "email",
                value: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "restaurantFoodItemsTable",
                keyColumn: "Id",
                keyValue: new Guid("7a4154a5-9e58-4df8-ae1a-bc02898f47ec"));

            migrationBuilder.DeleteData(
                table: "restaurantFoodItemsTable",
                keyColumn: "Id",
                keyValue: new Guid("d1426c2f-81d1-430c-9701-6542b246fdc9"));

            migrationBuilder.DeleteData(
                table: "restaurantTable",
                keyColumn: "Id",
                keyValue: new Guid("800a1f58-30f1-4b14-a612-aaf987bdc7fa"));

            migrationBuilder.DropColumn(
                name: "email",
                table: "userTable");

            migrationBuilder.InsertData(
                table: "restaurantFoodItemsTable",
                columns: new[] { "Id", "calories", "image", "name", "price", "restaurant_id", "weight" },
                values: new object[,]
                {
                    { new Guid("2fd96345-1feb-47b9-9db1-cd253c43334f"), 1000, "NONE", "Тестовое блюдо 2", 1200L, new Guid("54d33061-3691-4b7d-a60c-c53ef2e4eb4e"), 100 },
                    { new Guid("977a4622-7940-4269-8b72-c02892702b3f"), 2000, "NONE", "Тестовое блюдо", 1000L, new Guid("54d33061-3691-4b7d-a60c-c53ef2e4eb4e"), 100 }
                });

            migrationBuilder.InsertData(
                table: "restaurantTable",
                columns: new[] { "Id", "address", "close_time", "description", "imagePath", "open_time", "phone_number", "restaurantName", "status", "user_id" },
                values: new object[] { new Guid("fda6f2af-8dc4-4db3-95d5-8bf47130c92b"), "ул. Шолмова 3", "20:00", "Хороший тестовый ресторан", "NONE", "10:00", "+78004444434", "Тестовый ресторан 2", 1, new Guid("1993856e-2f5c-4790-a3d4-33e6a5718b47") });
        }
    }
}
