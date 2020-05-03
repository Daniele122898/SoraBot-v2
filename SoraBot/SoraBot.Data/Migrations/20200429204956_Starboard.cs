using Microsoft.EntityFrameworkCore.Migrations;

namespace SoraBot.Data.Migrations
{
    public partial class Starboard : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StarboardMessages",
                columns: table => new
                {
                    MessageId = table.Column<ulong>(nullable: false),
                    PostedMsgId = table.Column<ulong>(nullable: false),
                    GuildId = table.Column<ulong>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StarboardMessages", x => x.MessageId);
                    table.ForeignKey(
                        name: "FK_StarboardMessages_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Starboards",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(nullable: false),
                    StarboardChannelId = table.Column<ulong>(nullable: false),
                    StarboardThreshold = table.Column<uint>(nullable: false, defaultValue: 1u)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Starboards", x => x.GuildId);
                    table.ForeignKey(
                        name: "FK_Starboards_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StarboardMessages_GuildId",
                table: "StarboardMessages",
                column: "GuildId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StarboardMessages");

            migrationBuilder.DropTable(
                name: "Starboards");
        }
    }
}
