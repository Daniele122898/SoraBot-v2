using Microsoft.EntityFrameworkCore.Migrations;

namespace SoraBot.Data.Migrations
{
    public partial class FavoriteWaifus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FavoriteWaifuId",
                table: "Users",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_FavoriteWaifuId",
                table: "Users",
                column: "FavoriteWaifuId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Waifus_FavoriteWaifuId",
                table: "Users",
                column: "FavoriteWaifuId",
                principalTable: "Waifus",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Waifus_FavoriteWaifuId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_FavoriteWaifuId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "FavoriteWaifuId",
                table: "Users");
        }
    }
}
