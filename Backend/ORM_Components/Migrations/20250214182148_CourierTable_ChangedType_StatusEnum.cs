using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ORM_Components.Migrations
{
    public partial class CourierTable_ChangedType_StatusEnum : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "address",
                table: "restaurantTable",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "phone_number",
                table: "restaurantTable",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "restaurantName",
                table: "restaurantTable",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "status",
                table: "courierTable",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "address",
                table: "restaurantTable");

            migrationBuilder.DropColumn(
                name: "phone_number",
                table: "restaurantTable");

            migrationBuilder.DropColumn(
                name: "restaurantName",
                table: "restaurantTable");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "courierTable",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }
    }
}
