using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace SoraBotv2.Migrations
{
    public partial class Announcements : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<ulong>(
                name: "LeaveChannelId",
                table: "Guilds",
                type: "bigint unsigned",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.AddColumn<string>(
                name: "LeaveMessage",
                table: "Guilds",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<ulong>(
                name: "WelcomeChannelId",
                table: "Guilds",
                type: "bigint unsigned",
                nullable: false,
                defaultValue: 0ul);

            migrationBuilder.AddColumn<string>(
                name: "WelcomeMessage",
                table: "Guilds",
                type: "longtext",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LeaveChannelId",
                table: "Guilds");

            migrationBuilder.DropColumn(
                name: "LeaveMessage",
                table: "Guilds");

            migrationBuilder.DropColumn(
                name: "WelcomeChannelId",
                table: "Guilds");

            migrationBuilder.DropColumn(
                name: "WelcomeMessage",
                table: "Guilds");
        }
    }
}
