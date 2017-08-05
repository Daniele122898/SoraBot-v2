using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SoraBotv2.Migrations
{
    public partial class AddedAFK : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Afk",
                columns: table => new
                {
                    AfkId = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGeneratedOnAdd", true),
                    IsAfk = table.Column<bool>(nullable: false),
                    Message = table.Column<string>(nullable: true),
                    TimeToTriggerAgain = table.Column<DateTime>(nullable: false),
                    UserForeignId = table.Column<ulong>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Afk", x => x.AfkId);
                    table.ForeignKey(
                        name: "FK_Afk_Users_UserForeignId",
                        column: x => x.UserForeignId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Afk_UserForeignId",
                table: "Afk",
                column: "UserForeignId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Afk");
        }
    }
}
