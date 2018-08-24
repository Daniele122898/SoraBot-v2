using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace SoraBotv2.Migrations
{
    public partial class unboxWaifu : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserWaifus_Waifus_WaifuForeignId",
                table: "UserWaifus");

            migrationBuilder.DropForeignKey(
                name: "FK_UserWaifus_Users_WaifuForeignId",
                table: "UserWaifus");

            migrationBuilder.DropIndex(
                name: "IX_UserWaifus_WaifuForeignId",
                table: "UserWaifus");

            migrationBuilder.DropColumn(
                name: "WaifuForeignId",
                table: "UserWaifus");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Waifus",
                type: "int",
                nullable: false,
                oldClrType: typeof(ulong))
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<int>(
                name: "WaifuId",
                table: "UserWaifus",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_UserWaifus_UserForeignId",
                table: "UserWaifus",
                column: "UserForeignId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserWaifus_Users_UserForeignId",
                table: "UserWaifus",
                column: "UserForeignId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserWaifus_Users_UserForeignId",
                table: "UserWaifus");

            migrationBuilder.DropIndex(
                name: "IX_UserWaifus_UserForeignId",
                table: "UserWaifus");

            migrationBuilder.DropColumn(
                name: "WaifuId",
                table: "UserWaifus");

            migrationBuilder.AlterColumn<ulong>(
                name: "Id",
                table: "Waifus",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<ulong>(
                name: "WaifuForeignId",
                table: "UserWaifus",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.CreateIndex(
                name: "IX_UserWaifus_WaifuForeignId",
                table: "UserWaifus",
                column: "WaifuForeignId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserWaifus_Waifus_WaifuForeignId",
                table: "UserWaifus",
                column: "WaifuForeignId",
                principalTable: "Waifus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserWaifus_Users_WaifuForeignId",
                table: "UserWaifus",
                column: "WaifuForeignId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
