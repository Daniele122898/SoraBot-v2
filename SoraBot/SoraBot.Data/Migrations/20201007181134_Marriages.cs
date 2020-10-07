using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SoraBot.Data.Migrations
{
    public partial class Marriages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Marriages",
                columns: table => new
                {
                    Partner1Id = table.Column<ulong>(nullable: false),
                    Partner2Id = table.Column<ulong>(nullable: false),
                    PartnerSince = table.Column<DateTime>(nullable: false, defaultValue: new DateTime(2020, 10, 7, 18, 11, 33, 882, DateTimeKind.Utc).AddTicks(34))
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Marriages", x => new { x.Partner1Id, x.Partner2Id });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Marriages");
        }
    }
}
