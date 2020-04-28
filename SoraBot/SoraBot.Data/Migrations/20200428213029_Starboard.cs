using Microsoft.EntityFrameworkCore.Migrations;

namespace SoraBot.Data.Migrations
{
    public partial class Starboard : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<ulong>(
                name: "StarboardChannelId",
                table: "Guilds",
                nullable: true);

            migrationBuilder.AddColumn<uint>(
                name: "StarboardThreshold",
                table: "Guilds",
                nullable: false,
                defaultValue: 1u);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StarboardChannelId",
                table: "Guilds");

            migrationBuilder.DropColumn(
                name: "StarboardThreshold",
                table: "Guilds");
        }
    }
}
