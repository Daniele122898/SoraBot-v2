using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SoraBotv2.Migrations
{
    public partial class TrackGivenInteractions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "High5Given",
                table: "Interactions",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "HugsGiven",
                table: "Interactions",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "KissesGiven",
                table: "Interactions",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PatsGiven",
                table: "Interactions",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PokesGiven",
                table: "Interactions",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PunchesGiven",
                table: "Interactions",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SlapsGiven",
                table: "Interactions",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "High5Given",
                table: "Interactions");

            migrationBuilder.DropColumn(
                name: "HugsGiven",
                table: "Interactions");

            migrationBuilder.DropColumn(
                name: "KissesGiven",
                table: "Interactions");

            migrationBuilder.DropColumn(
                name: "PatsGiven",
                table: "Interactions");

            migrationBuilder.DropColumn(
                name: "PokesGiven",
                table: "Interactions");

            migrationBuilder.DropColumn(
                name: "PunchesGiven",
                table: "Interactions");

            migrationBuilder.DropColumn(
                name: "SlapsGiven",
                table: "Interactions");
        }
    }
}
