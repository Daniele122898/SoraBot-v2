using Microsoft.EntityFrameworkCore.Migrations;

namespace SoraBot.Data.Migrations
{
    public partial class UserExp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<uint>(
                name: "Exp",
                table: "Users",
                nullable: false,
                defaultValue: 0u);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Exp",
                table: "Users");
        }
    }
}
