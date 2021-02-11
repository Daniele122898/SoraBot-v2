using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SoraBot.Data.Migrations
{
    public partial class Clans : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "PartnerSince",
                table: "Marriages",
                nullable: false,
                defaultValue: new DateTime(2021, 2, 11, 22, 26, 2, 221, DateTimeKind.Utc).AddTicks(5456),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2020, 10, 7, 18, 11, 33, 882, DateTimeKind.Utc).AddTicks(34));

            migrationBuilder.CreateTable(
                name: "Clans",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: false),
                    OwnerId = table.Column<ulong>(nullable: false),
                    AvatarUrl = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: true),
                    Created = table.Column<DateTime>(nullable: false),
                    Level = table.Column<int>(nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Clans_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClanInvites",
                columns: table => new
                {
                    ClanId = table.Column<int>(nullable: false),
                    UserId = table.Column<ulong>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClanInvites", x => new { x.ClanId, x.UserId });
                    table.ForeignKey(
                        name: "FK_ClanInvites_Clans_ClanId",
                        column: x => x.ClanId,
                        principalTable: "Clans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClanInvites_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClanMembers",
                columns: table => new
                {
                    UserId = table.Column<ulong>(nullable: false),
                    ClanId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClanMembers", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_ClanMembers_Clans_ClanId",
                        column: x => x.ClanId,
                        principalTable: "Clans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClanMembers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClanInvites_UserId",
                table: "ClanInvites",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ClanMembers_ClanId",
                table: "ClanMembers",
                column: "ClanId");

            migrationBuilder.CreateIndex(
                name: "IX_Clans_OwnerId",
                table: "Clans",
                column: "OwnerId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClanInvites");

            migrationBuilder.DropTable(
                name: "ClanMembers");

            migrationBuilder.DropTable(
                name: "Clans");

            migrationBuilder.AlterColumn<DateTime>(
                name: "PartnerSince",
                table: "Marriages",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2020, 10, 7, 18, 11, 33, 882, DateTimeKind.Utc).AddTicks(34),
                oldClrType: typeof(DateTime),
                oldDefaultValue: new DateTime(2021, 2, 11, 22, 26, 2, 221, DateTimeKind.Utc).AddTicks(5456));
        }
    }
}
