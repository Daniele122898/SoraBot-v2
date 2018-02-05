using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace SoraBotv2.Migrations
{
    public partial class ClanChange : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClanInvites_Clans_ClanName",
                table: "ClanInvites");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Clans_ClanName",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_ClanName",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Clans",
                table: "Clans");

            migrationBuilder.DropIndex(
                name: "IX_ClanInvites_ClanName",
                table: "ClanInvites");

            migrationBuilder.AlterColumn<string>(
                name: "ClanName",
                table: "Users",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ClanId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Clans",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string));

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "Clans",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<string>(
                name: "ClanName",
                table: "ClanInvites",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Clans",
                table: "Clans",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Users_ClanId",
                table: "Users",
                column: "ClanId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Clans_ClanId",
                table: "Users",
                column: "ClanId",
                principalTable: "Clans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Clans_ClanId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_ClanId",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Clans",
                table: "Clans");

            migrationBuilder.DropColumn(
                name: "ClanId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Clans");

            migrationBuilder.AlterColumn<string>(
                name: "ClanName",
                table: "Users",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Clans",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ClanName",
                table: "ClanInvites",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Clans",
                table: "Clans",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Users_ClanName",
                table: "Users",
                column: "ClanName");

            migrationBuilder.CreateIndex(
                name: "IX_ClanInvites_ClanName",
                table: "ClanInvites",
                column: "ClanName");

            migrationBuilder.AddForeignKey(
                name: "FK_ClanInvites_Clans_ClanName",
                table: "ClanInvites",
                column: "ClanName",
                principalTable: "Clans",
                principalColumn: "Name",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Clans_ClanName",
                table: "Users",
                column: "ClanName",
                principalTable: "Clans",
                principalColumn: "Name",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
