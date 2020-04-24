using Microsoft.EntityFrameworkCore.Migrations;

namespace SoraBot.Data.Migrations
{
    public partial class CustomBg : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasCustomProfileBg",
                table: "Users",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasCustomProfileBg",
                table: "Users");
        }
    }
}
