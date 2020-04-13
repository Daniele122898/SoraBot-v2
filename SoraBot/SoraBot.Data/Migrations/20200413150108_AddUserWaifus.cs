using Microsoft.EntityFrameworkCore.Migrations;

namespace SoraBot.Data.Migrations
{
    public partial class AddUserWaifus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserWaifu",
                columns: table => new
                {
                    UserId = table.Column<ulong>(nullable: false),
                    WaifuId = table.Column<int>(nullable: false),
                    Count = table.Column<uint>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserWaifu", x => new { x.UserId, x.WaifuId });
                    table.ForeignKey(
                        name: "FK_UserWaifu_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserWaifu_Waifus_WaifuId",
                        column: x => x.WaifuId,
                        principalTable: "Waifus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserWaifu_WaifuId",
                table: "UserWaifu",
                column: "WaifuId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserWaifu");
        }
    }
}
