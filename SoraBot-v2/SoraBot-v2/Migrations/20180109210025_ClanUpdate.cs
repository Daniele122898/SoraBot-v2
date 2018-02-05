using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace SoraBotv2.Migrations
{
    public partial class ClanUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClanName",
                table: "Users",
                type: "varchar(127)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ClanStaff",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Clans",
                columns: table => new
                {
                    Name = table.Column<string>(type: "varchar(127)", nullable: false),
                    HasImage = table.Column<bool>(type: "bit", nullable: false),
                    OwnerId = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clans", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "GuildUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Exp = table.Column<float>(type: "float", nullable: false),
                    GuildId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    UserId = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuildUsers_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "GuildId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClanInvites",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ClanName = table.Column<string>(type: "varchar(127)", nullable: true),
                    StaffId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    UserId = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClanInvites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClanInvites_Clans_ClanName",
                        column: x => x.ClanName,
                        principalTable: "Clans",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_ClanName",
                table: "Users",
                column: "ClanName");

            migrationBuilder.CreateIndex(
                name: "IX_ClanInvites_ClanName",
                table: "ClanInvites",
                column: "ClanName");

            migrationBuilder.CreateIndex(
                name: "IX_GuildUsers_GuildId",
                table: "GuildUsers",
                column: "GuildId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Clans_ClanName",
                table: "Users",
                column: "ClanName",
                principalTable: "Clans",
                principalColumn: "Name",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Clans_ClanName",
                table: "Users");

            migrationBuilder.DropTable(
                name: "ClanInvites");

            migrationBuilder.DropTable(
                name: "GuildUsers");

            migrationBuilder.DropTable(
                name: "Clans");

            migrationBuilder.DropIndex(
                name: "IX_Users_ClanName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ClanName",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ClanStaff",
                table: "Users");
        }
    }
}
