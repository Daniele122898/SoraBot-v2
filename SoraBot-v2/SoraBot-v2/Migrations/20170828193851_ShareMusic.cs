using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace SoraBotv2.Migrations
{
    public partial class ShareMusic : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ShareCentrals",
                columns: table => new
                {
                    ShareLink = table.Column<string>(type: "varchar(127)", nullable: false),
                    CreatorId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    Downvotes = table.Column<int>(type: "int", nullable: false),
                    Tags = table.Column<string>(type: "longtext", nullable: true),
                    Titel = table.Column<string>(type: "longtext", nullable: true),
                    Upvotes = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShareCentrals", x => x.ShareLink);
                    table.ForeignKey(
                        name: "FK_ShareCentrals_Users_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Votings",
                columns: table => new
                {
                    VoteId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ShareLink = table.Column<string>(type: "varchar(127)", nullable: true),
                    UpOrDown = table.Column<bool>(type: "bit", nullable: false),
                    VoterId = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Votings", x => x.VoteId);
                    table.ForeignKey(
                        name: "FK_Votings_ShareCentrals_ShareLink",
                        column: x => x.ShareLink,
                        principalTable: "ShareCentrals",
                        principalColumn: "ShareLink",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Votings_Users_VoterId",
                        column: x => x.VoterId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShareCentrals_CreatorId",
                table: "ShareCentrals",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Votings_ShareLink",
                table: "Votings",
                column: "ShareLink");

            migrationBuilder.CreateIndex(
                name: "IX_Votings_VoterId",
                table: "Votings",
                column: "VoterId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Votings");

            migrationBuilder.DropTable(
                name: "ShareCentrals");
        }
    }
}
