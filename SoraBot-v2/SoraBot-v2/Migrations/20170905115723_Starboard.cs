using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace SoraBotv2.Migrations
{
    public partial class Starboard : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<ulong>(
                name: "StarChannelId",
                table: "Guilds",
                type: "bigint unsigned",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.AddColumn<int>(
                name: "StarMinimum",
                table: "Guilds",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "StarMessages",
                columns: table => new
                {
                    MessageId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    GuildForeignId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    HitZeroCount = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    IsPosted = table.Column<bool>(type: "bit", nullable: false),
                    StarCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StarMessages", x => x.MessageId);
                    table.ForeignKey(
                        name: "FK_StarMessages_Guilds_GuildForeignId",
                        column: x => x.GuildForeignId,
                        principalTable: "Guilds",
                        principalColumn: "GuildId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StarMessages_GuildForeignId",
                table: "StarMessages",
                column: "GuildForeignId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StarMessages");

            migrationBuilder.DropColumn(
                name: "StarChannelId",
                table: "Guilds");

            migrationBuilder.DropColumn(
                name: "StarMinimum",
                table: "Guilds");
        }
    }
}
