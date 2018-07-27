using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace SoraBotv2.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Bans",
                columns: table => new
                {
                    UserId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    BannedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Reason = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bans", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "ClanInvites",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ClanName = table.Column<string>(type: "longtext", nullable: true),
                    StaffId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    UserId = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClanInvites", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Clans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    AvatarUrl = table.Column<string>(type: "longtext", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    HasImage = table.Column<bool>(type: "bit", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    Message = table.Column<string>(type: "longtext", nullable: true),
                    Name = table.Column<string>(type: "longtext", nullable: true),
                    OwnerId = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Guilds",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    DefaultRoleId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    EmbedLeave = table.Column<bool>(type: "bit", nullable: false),
                    EmbedWelcome = table.Column<bool>(type: "bit", nullable: false),
                    EnabledLvlUpMessage = table.Column<bool>(type: "bit", nullable: false),
                    HasDefaultRole = table.Column<bool>(type: "bit", nullable: false),
                    IsDjRestricted = table.Column<bool>(type: "bit", nullable: false),
                    LeaveChannelId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    LeaveMessage = table.Column<string>(type: "longtext", nullable: true),
                    LevelUpMessage = table.Column<string>(type: "longtext", nullable: true),
                    Prefix = table.Column<string>(type: "longtext", nullable: true),
                    PunishLogsId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    RestrictTags = table.Column<bool>(type: "bit", nullable: false),
                    SendLvlDm = table.Column<bool>(type: "bit", nullable: false),
                    StarChannelId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    StarMinimum = table.Column<int>(type: "int", nullable: false),
                    WelcomeChannelId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    WelcomeMessage = table.Column<string>(type: "longtext", nullable: true)
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
                name: "Waifus",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ImageUrl = table.Column<string>(type: "longtext", nullable: true),
                    Name = table.Column<string>(type: "longtext", nullable: true),
                    Rarity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Waifus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    CanGainAgain = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ClanId = table.Column<int>(type: "int", nullable: true),
                    ClanName = table.Column<string>(type: "longtext", nullable: true),
                    ClanStaff = table.Column<bool>(type: "bit", nullable: false),
                    Exp = table.Column<float>(type: "float", nullable: false),
                    HasBg = table.Column<bool>(type: "bit", nullable: false),
                    JoinedClan = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Money = table.Column<int>(type: "int", nullable: false),
                    NextDaily = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Notified = table.Column<bool>(type: "bit", nullable: false),
                    ShowProfileCardAgain = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdateBgAgain = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_Users_Clans_ClanId",
                        column: x => x.ClanId,
                        principalTable: "Clans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Cases",
                columns: table => new
                {
                    CaseId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CaseNr = table.Column<int>(type: "int", nullable: false),
                    GuildForeignId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    ModId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    PunishMsgId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    Reason = table.Column<string>(type: "longtext", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    UserNameDisc = table.Column<string>(type: "longtext", nullable: true),
                    WarnNr = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cases", x => x.CaseId);
                    table.ForeignKey(
                        name: "FK_Cases_Guilds_GuildForeignId",
                        column: x => x.GuildForeignId,
                        principalTable: "Guilds",
                        principalColumn: "GuildId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GuildLevelRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Banned = table.Column<bool>(type: "bit", nullable: false),
                    GuildId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    RequiredLevel = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildLevelRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuildLevelRoles_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "GuildId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GuildUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Exp = table.Column<float>(type: "float", nullable: false),
                    GuildId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    UserId = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuildUsers_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "GuildId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SelfAssignableRoles",
                columns: table => new
                {
                    RoleId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    CanExpire = table.Column<bool>(type: "bit", nullable: false),
                    Cost = table.Column<int>(type: "int", nullable: false),
                    Duration = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    GuildForeignId = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SelfAssignableRoles", x => x.RoleId);
                    table.ForeignKey(
                        name: "FK_SelfAssignableRoles_Guilds_GuildForeignId",
                        column: x => x.GuildForeignId,
                        principalTable: "Guilds",
                        principalColumn: "GuildId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StarMessages",
                columns: table => new
                {
                    MessageId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    GuildForeignId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    HitZeroCount = table.Column<byte>(type: "tinyint unsigned", nullable: false),
                    IsPosted = table.Column<bool>(type: "bit", nullable: false),
                    PostedMsgId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    StarCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StarMessages", x => x.MessageId);
                    table.ForeignKey(
                        name: "FK_StarMessages_Guilds_GuildForeignId",
                        column: x => x.GuildForeignId,
                        principalTable: "Guilds",
                        principalColumn: "GuildId",
                        onDelete: ReferentialAction.Cascade);
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

            migrationBuilder.CreateTable(
                name: "ShareCentrals",
                columns: table => new
                {
                    ShareLink = table.Column<string>(type: "varchar(127)", nullable: false),
                    CreatorId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    Downvotes = table.Column<int>(type: "int", nullable: false),
                    IsPrivate = table.Column<bool>(type: "bit", nullable: false),
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
                name: "UserWaifus",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Count = table.Column<int>(type: "int", nullable: false),
                    UserForeignId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    WaifuForeignId = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserWaifus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserWaifus_Waifus_WaifuForeignId",
                        column: x => x.WaifuForeignId,
                        principalTable: "Waifus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserWaifus_Users_WaifuForeignId",
                        column: x => x.WaifuForeignId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExpiringRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ExpiresAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    GuildForeignId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    RoleForeignId = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    UserForeignId = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpiringRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExpiringRoles_Guilds_GuildForeignId",
                        column: x => x.GuildForeignId,
                        principalTable: "Guilds",
                        principalColumn: "GuildId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExpiringRoles_SelfAssignableRoles_RoleForeignId",
                        column: x => x.RoleForeignId,
                        principalTable: "SelfAssignableRoles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExpiringRoles_Users_UserForeignId",
                        column: x => x.UserForeignId,
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
                name: "IX_Afk_UserForeignId",
                table: "Afk",
                column: "UserForeignId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cases_GuildForeignId",
                table: "Cases",
                column: "GuildForeignId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpiringRoles_GuildForeignId",
                table: "ExpiringRoles",
                column: "GuildForeignId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpiringRoles_RoleForeignId",
                table: "ExpiringRoles",
                column: "RoleForeignId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpiringRoles_UserForeignId",
                table: "ExpiringRoles",
                column: "UserForeignId");

            migrationBuilder.CreateIndex(
                name: "IX_GuildLevelRoles_GuildId",
                table: "GuildLevelRoles",
                column: "GuildId");

            migrationBuilder.CreateIndex(
                name: "IX_GuildUsers_GuildId",
                table: "GuildUsers",
                column: "GuildId");

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
                name: "IX_SelfAssignableRoles_GuildForeignId",
                table: "SelfAssignableRoles",
                column: "GuildForeignId");

            migrationBuilder.CreateIndex(
                name: "IX_ShareCentrals_CreatorId",
                table: "ShareCentrals",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_StarMessages_GuildForeignId",
                table: "StarMessages",
                column: "GuildForeignId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_GuildForeignId",
                table: "Tags",
                column: "GuildForeignId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_ClanId",
                table: "Users",
                column: "ClanId");

            migrationBuilder.CreateIndex(
                name: "IX_UserWaifus_WaifuForeignId",
                table: "UserWaifus",
                column: "WaifuForeignId");

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
                name: "Afk");

            migrationBuilder.DropTable(
                name: "Bans");

            migrationBuilder.DropTable(
                name: "Cases");

            migrationBuilder.DropTable(
                name: "ClanInvites");

            migrationBuilder.DropTable(
                name: "ExpiringRoles");

            migrationBuilder.DropTable(
                name: "GuildLevelRoles");

            migrationBuilder.DropTable(
                name: "GuildUsers");

            migrationBuilder.DropTable(
                name: "Interactions");

            migrationBuilder.DropTable(
                name: "Marriages");

            migrationBuilder.DropTable(
                name: "Reminders");

            migrationBuilder.DropTable(
                name: "Songs");

            migrationBuilder.DropTable(
                name: "StarMessages");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "UserWaifus");

            migrationBuilder.DropTable(
                name: "Votings");

            migrationBuilder.DropTable(
                name: "SelfAssignableRoles");

            migrationBuilder.DropTable(
                name: "Waifus");

            migrationBuilder.DropTable(
                name: "ShareCentrals");

            migrationBuilder.DropTable(
                name: "Guilds");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Clans");
        }
    }
}
