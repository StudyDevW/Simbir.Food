using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ORM_Components.Migrations
{
    public partial class AddedChatIdInUserTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "chatId",
                table: "userTable",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "chatId",
                table: "userTable");
        }
    }
}
