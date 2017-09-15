using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace SoraBotv2.Migrations
{
    public partial class ModFix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserNameDisc",
                table: "Cases",
                type: "longtext",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WarnNr",
                table: "Cases",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserNameDisc",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "WarnNr",
                table: "Cases");
        }
    }
}
