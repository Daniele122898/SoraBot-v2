using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

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
                    UserId = table.Column<ulong>(nullable: false),
                    Reason = table.Column<string>(nullable: true),
                    BannedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bans", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "ClanInvites",
                columns: table => new
                {
                    Id = table.Column<ulong>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    StaffId = table.Column<ulong>(nullable: false),
                    UserId = table.Column<ulong>(nullable: false),
                    ClanName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClanInvites", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Clans",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true),
                    OwnerId = table.Column<ulong>(nullable: false),
                    HasImage = table.Column<bool>(nullable: false),
                    AvatarUrl = table.Column<string>(nullable: true),
                    Message = table.Column<string>(nullable: true),
                    Created = table.Column<DateTime>(nullable: false),
                    Level = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Guilds",
                columns: table => new
                {
                    GuildId = table.Column<ulong>(nullable: false),
                    Prefix = table.Column<string>(nullable: true),
                    RestrictTags = table.Column<bool>(nullable: false),
                    IsDjRestricted = table.Column<bool>(nullable: false),
                    NeedVotes = table.Column<bool>(nullable: false),
                    StarChannelId = table.Column<ulong>(nullable: false),
                    StarMinimum = table.Column<int>(nullable: false),
                    HasDefaultRole = table.Column<bool>(nullable: false),
                    DefaultRoleId = table.Column<ulong>(nullable: false),
                    WelcomeChannelId = table.Column<ulong>(nullable: false),
                    LeaveChannelId = table.Column<ulong>(nullable: false),
                    WelcomeMessage = table.Column<string>(nullable: true),
                    LeaveMessage = table.Column<string>(nullable: true),
                    EmbedWelcome = table.Column<bool>(nullable: false),
                    EmbedLeave = table.Column<bool>(nullable: false),
                    PunishLogsId = table.Column<ulong>(nullable: false),
                    LevelUpMessage = table.Column<string>(nullable: true),
                    EnabledLvlUpMessage = table.Column<bool>(nullable: false),
                    SendLvlDm = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guilds", x => x.GuildId);
                });

            migrationBuilder.CreateTable(
                name: "RequestLogs",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Accepted = table.Column<bool>(nullable: false),
                    UserId = table.Column<ulong>(nullable: false),
                    WaifuName = table.Column<string>(nullable: true),
                    ProcessedTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Songs",
                columns: table => new
                {
                    Base64EncodedLink = table.Column<string>(nullable: false),
                    Added = table.Column<DateTime>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    RequestorUserId = table.Column<ulong>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Songs", x => x.Base64EncodedLink);
                });

            migrationBuilder.CreateTable(
                name: "WaifuRequests",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true),
                    ImageUrl = table.Column<string>(nullable: true),
                    Rarity = table.Column<short>(nullable: false),
                    UserId = table.Column<ulong>(nullable: false),
                    TimeStamp = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WaifuRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Waifus",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true),
                    ImageUrl = table.Column<string>(nullable: true),
                    Rarity = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Waifus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<ulong>(nullable: false),
                    Exp = table.Column<float>(nullable: false),
                    CanGainAgain = table.Column<DateTime>(nullable: false),
                    Notified = table.Column<bool>(nullable: false),
                    HasBg = table.Column<bool>(nullable: false),
                    UpdateBgAgain = table.Column<DateTime>(nullable: false),
                    ShowProfileCardAgain = table.Column<DateTime>(nullable: false),
                    Money = table.Column<int>(nullable: false),
                    NextDaily = table.Column<DateTime>(nullable: false),
                    FavoriteWaifu = table.Column<int>(nullable: false),
                    NotifyOnWaifuRequest = table.Column<bool>(nullable: false),
                    ClanName = table.Column<string>(nullable: true),
                    ClanStaff = table.Column<bool>(nullable: false),
                    JoinedClan = table.Column<DateTime>(nullable: false),
                    ClanId = table.Column<int>(nullable: true)
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
                    CaseId = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Type = table.Column<int>(nullable: false),
                    CaseNr = table.Column<int>(nullable: false),
                    ModId = table.Column<ulong>(nullable: false),
                    UserId = table.Column<ulong>(nullable: false),
                    UserNameDisc = table.Column<string>(nullable: true),
                    Reason = table.Column<string>(nullable: true),
                    PunishMsgId = table.Column<ulong>(nullable: false),
                    WarnNr = table.Column<int>(nullable: false),
                    GuildForeignId = table.Column<ulong>(nullable: false)
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
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RoleId = table.Column<ulong>(nullable: false),
                    RequiredLevel = table.Column<int>(nullable: false),
                    Banned = table.Column<bool>(nullable: false),
                    GuildId = table.Column<ulong>(nullable: false)
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
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<ulong>(nullable: false),
                    Exp = table.Column<float>(nullable: false),
                    GuildId = table.Column<ulong>(nullable: false)
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
                    RoleId = table.Column<ulong>(nullable: false),
                    Cost = table.Column<int>(nullable: false),
                    Duration = table.Column<TimeSpan>(nullable: false),
                    CanExpire = table.Column<bool>(nullable: false),
                    GuildForeignId = table.Column<ulong>(nullable: false)
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
                    MessageId = table.Column<ulong>(nullable: false),
                    StarCount = table.Column<int>(nullable: false),
                    HitZeroCount = table.Column<byte>(nullable: false),
                    IsPosted = table.Column<bool>(nullable: false),
                    PostedMsgId = table.Column<ulong>(nullable: false),
                    GuildForeignId = table.Column<ulong>(nullable: false)
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
                    TagId = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(nullable: true),
                    Value = table.Column<string>(nullable: true),
                    CreatorId = table.Column<ulong>(nullable: false),
                    PictureAttachment = table.Column<bool>(nullable: false),
                    AttachmentString = table.Column<string>(nullable: true),
                    ForceEmbed = table.Column<bool>(nullable: false),
                    GuildForeignId = table.Column<ulong>(nullable: false)
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
                    AfkId = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    IsAfk = table.Column<bool>(nullable: false),
                    TimeToTriggerAgain = table.Column<DateTime>(nullable: false),
                    Message = table.Column<string>(nullable: true),
                    UserForeignId = table.Column<ulong>(nullable: false)
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
                    InteractionsId = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Pats = table.Column<int>(nullable: false),
                    Hugs = table.Column<int>(nullable: false),
                    Kisses = table.Column<int>(nullable: false),
                    High5 = table.Column<int>(nullable: false),
                    Pokes = table.Column<int>(nullable: false),
                    Slaps = table.Column<int>(nullable: false),
                    Punches = table.Column<int>(nullable: false),
                    PatsGiven = table.Column<int>(nullable: false),
                    HugsGiven = table.Column<int>(nullable: false),
                    KissesGiven = table.Column<int>(nullable: false),
                    High5Given = table.Column<int>(nullable: false),
                    PokesGiven = table.Column<int>(nullable: false),
                    SlapsGiven = table.Column<int>(nullable: false),
                    PunchesGiven = table.Column<int>(nullable: false),
                    UserForeignId = table.Column<ulong>(nullable: false)
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
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Since = table.Column<DateTime>(nullable: false),
                    PartnerId = table.Column<ulong>(nullable: false),
                    UserForeignId = table.Column<ulong>(nullable: false)
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
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Time = table.Column<DateTime>(nullable: false),
                    Message = table.Column<string>(nullable: true),
                    UserForeignId = table.Column<ulong>(nullable: false)
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
                    ShareLink = table.Column<string>(nullable: false),
                    Upvotes = table.Column<int>(nullable: false),
                    Downvotes = table.Column<int>(nullable: false),
                    Tags = table.Column<string>(nullable: true),
                    Titel = table.Column<string>(nullable: true),
                    IsPrivate = table.Column<bool>(nullable: false),
                    CreatorId = table.Column<ulong>(nullable: false)
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
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Count = table.Column<int>(nullable: false),
                    WaifuId = table.Column<int>(nullable: false),
                    UserForeignId = table.Column<ulong>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserWaifus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserWaifus_Users_UserForeignId",
                        column: x => x.UserForeignId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExpiringRoles",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ExpiresAt = table.Column<DateTime>(nullable: false),
                    UserForeignId = table.Column<ulong>(nullable: false),
                    RoleForeignId = table.Column<ulong>(nullable: false),
                    GuildForeignId = table.Column<ulong>(nullable: false)
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
                    VoteId = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ShareLink = table.Column<string>(nullable: true),
                    VoterId = table.Column<ulong>(nullable: false),
                    UpOrDown = table.Column<bool>(nullable: false)
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
                name: "IX_UserWaifus_UserForeignId",
                table: "UserWaifus",
                column: "UserForeignId");

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
                name: "RequestLogs");

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
                name: "WaifuRequests");

            migrationBuilder.DropTable(
                name: "Waifus");

            migrationBuilder.DropTable(
                name: "SelfAssignableRoles");

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
