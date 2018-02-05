using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace SoraBotv2.Migrations
{
    public partial class GuildLevelRoles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EnabledLvlUpMessage",
                table: "Guilds",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LevelUpMessage",
                table: "Guilds",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SendLvlDm",
                table: "Guilds",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "GuildLevelRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Banned = table.Column<bool>(type: "bit", nullable: false),
                    GuildId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    RequiredLevel = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildLevelRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuildLevelRoles_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "GuildId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GuildLevelRoles_GuildId",
                table: "GuildLevelRoles",
                column: "GuildId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GuildLevelRoles");

            migrationBuilder.DropColumn(
                name: "EnabledLvlUpMessage",
                table: "Guilds");

            migrationBuilder.DropColumn(
                name: "LevelUpMessage",
                table: "Guilds");

            migrationBuilder.DropColumn(
                name: "SendLvlDm",
                table: "Guilds");
        }
    }
}
