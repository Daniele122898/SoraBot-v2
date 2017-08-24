using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace SoraBotv2.Migrations
{
    public partial class newInitial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Guilds",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    Prefix = table.Column<string>(type: "longtext", nullable: true),
                    RestrictTags = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guilds", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "Songs",
                columns: table => new
                {
                    Base64EncodedLink = table.Column<string>(type: "varchar(127)", nullable: false),
                    Added = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: true),
                    RequestorUserId = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Songs", x => x.Base64EncodedLink);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    CanGainAgain = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Exp = table.Column<float>(type: "float", nullable: false),
                    HasBg = table.Column<bool>(type: "bit", nullable: false),
                    Notified = table.Column<bool>(type: "bit", nullable: false),
                    ShowProfileCardAgain = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdateBgAgain = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    TagId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AttachmentString = table.Column<string>(type: "longtext", nullable: true),
                    CreatorId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    ForceEmbed = table.Column<bool>(type: "bit", nullable: false),
                    GuildForeignId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    Name = table.Column<string>(type: "longtext", nullable: true),
                    PictureAttachment = table.Column<bool>(type: "bit", nullable: false),
                    Value = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.TagId);
                    table.ForeignKey(
                        name: "FK_Tags_Guilds_GuildForeignId",
                        column: x => x.GuildForeignId,
                        principalTable: "Guilds",
                        principalColumn: "GuildId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Afk",
                columns: table => new
                {
                    AfkId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IsAfk = table.Column<bool>(type: "bit", nullable: false),
                    Message = table.Column<string>(type: "longtext", nullable: true),
                    TimeToTriggerAgain = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UserForeignId = table.Column<ulong>(type: "bigint unsigned", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "Interactions",
                columns: table => new
                {
                    InteractionsId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    High5 = table.Column<int>(type: "int", nullable: false),
                    High5Given = table.Column<int>(type: "int", nullable: false),
                    Hugs = table.Column<int>(type: "int", nullable: false),
                    HugsGiven = table.Column<int>(type: "int", nullable: false),
                    Kisses = table.Column<int>(type: "int", nullable: false),
                    KissesGiven = table.Column<int>(type: "int", nullable: false),
                    Pats = table.Column<int>(type: "int", nullable: false),
                    PatsGiven = table.Column<int>(type: "int", nullable: false),
                    Pokes = table.Column<int>(type: "int", nullable: false),
                    PokesGiven = table.Column<int>(type: "int", nullable: false),
                    Punches = table.Column<int>(type: "int", nullable: false),
                    PunchesGiven = table.Column<int>(type: "int", nullable: false),
                    Slaps = table.Column<int>(type: "int", nullable: false),
                    SlapsGiven = table.Column<int>(type: "int", nullable: false),
                    UserForeignId = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Interactions", x => x.InteractionsId);
                    table.ForeignKey(
                        name: "FK_Interactions_Users_UserForeignId",
                        column: x => x.UserForeignId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Marriages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PartnerId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    Since = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UserForeignId = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Marriages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Marriages_Users_UserForeignId",
                        column: x => x.UserForeignId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reminders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Message = table.Column<string>(type: "longtext", nullable: true),
                    Time = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UserForeignId = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reminders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reminders_Users_UserForeignId",
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

            migrationBuilder.CreateIndex(
                name: "IX_Interactions_UserForeignId",
                table: "Interactions",
                column: "UserForeignId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Marriages_UserForeignId",
                table: "Marriages",
                column: "UserForeignId");

            migrationBuilder.CreateIndex(
                name: "IX_Reminders_UserForeignId",
                table: "Reminders",
                column: "UserForeignId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_GuildForeignId",
                table: "Tags",
                column: "GuildForeignId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Afk");

            migrationBuilder.DropTable(
                name: "Interactions");

            migrationBuilder.DropTable(
                name: "Marriages");

            migrationBuilder.DropTable(
                name: "Reminders");

            migrationBuilder.DropTable(
                name: "Songs");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Guilds");
        }
    }
}
