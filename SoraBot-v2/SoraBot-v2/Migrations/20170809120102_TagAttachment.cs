using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SoraBotv2.Migrations
{
    public partial class TagAttachment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AttachmentString",
                table: "Tags",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PictureAttachment",
                table: "Tags",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttachmentString",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "PictureAttachment",
                table: "Tags");
        }
    }
}
