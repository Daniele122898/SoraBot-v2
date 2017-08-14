using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using SoraBot_v2.Data;

namespace SoraBotv2.Migrations
{
    [DbContext(typeof(SoraContext))]
    [Migration("20170813165251_UserEXP")]
    partial class UserEXP
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.2");

            modelBuilder.Entity("SoraBot_v2.Data.Entities.Guild", b =>
                {
                    b.Property<ulong>("GuildId");

                    b.Property<string>("Prefix");

                    b.Property<bool>("RestrictTags");

                    b.HasKey("GuildId");

                    b.ToTable("Guilds");
                });

            modelBuilder.Entity("SoraBot_v2.Data.Entities.SubEntities.Afk", b =>
                {
                    b.Property<int>("AfkId")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("IsAfk");

                    b.Property<string>("Message");

                    b.Property<DateTime>("TimeToTriggerAgain");

                    b.Property<ulong>("UserForeignId");

                    b.HasKey("AfkId");

                    b.HasIndex("UserForeignId")
                        .IsUnique();

                    b.ToTable("Afk");
                });

            modelBuilder.Entity("SoraBot_v2.Data.Entities.SubEntities.Interactions", b =>
                {
                    b.Property<int>("InteractionsId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("High5");

                    b.Property<int>("High5Given");

                    b.Property<int>("Hugs");

                    b.Property<int>("HugsGiven");

                    b.Property<int>("Kisses");

                    b.Property<int>("KissesGiven");

                    b.Property<int>("Pats");

                    b.Property<int>("PatsGiven");

                    b.Property<int>("Pokes");

                    b.Property<int>("PokesGiven");

                    b.Property<int>("Punches");

                    b.Property<int>("PunchesGiven");

                    b.Property<int>("Slaps");

                    b.Property<int>("SlapsGiven");

                    b.Property<ulong>("UserForeignId");

                    b.HasKey("InteractionsId");

                    b.HasIndex("UserForeignId")
                        .IsUnique();

                    b.ToTable("Interactions");
                });

            modelBuilder.Entity("SoraBot_v2.Data.Entities.SubEntities.Tags", b =>
                {
                    b.Property<int>("TagId")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AttachmentString");

                    b.Property<ulong>("CreatorId");

                    b.Property<bool>("ForceEmbed");

                    b.Property<ulong>("GuildForeignId");

                    b.Property<string>("Name");

                    b.Property<bool>("PictureAttachment");

                    b.Property<string>("Value");

                    b.HasKey("TagId");

                    b.HasIndex("GuildForeignId");

                    b.ToTable("Tags");
                });

            modelBuilder.Entity("SoraBot_v2.Data.Entities.User", b =>
                {
                    b.Property<ulong>("UserId");

                    b.Property<string>("BgFileName");

                    b.Property<DateTime>("CanGainAgain");

                    b.Property<float>("Exp");

                    b.Property<bool>("HasBg");

                    b.Property<bool>("Notified");

                    b.Property<DateTime>("UpdateBgAgain");

                    b.HasKey("UserId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("SoraBot_v2.Data.Entities.SubEntities.Afk", b =>
                {
                    b.HasOne("SoraBot_v2.Data.Entities.User", "User")
                        .WithOne("Afk")
                        .HasForeignKey("SoraBot_v2.Data.Entities.SubEntities.Afk", "UserForeignId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("SoraBot_v2.Data.Entities.SubEntities.Interactions", b =>
                {
                    b.HasOne("SoraBot_v2.Data.Entities.User", "User")
                        .WithOne("Interactions")
                        .HasForeignKey("SoraBot_v2.Data.Entities.SubEntities.Interactions", "UserForeignId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("SoraBot_v2.Data.Entities.SubEntities.Tags", b =>
                {
                    b.HasOne("SoraBot_v2.Data.Entities.Guild", "Guild")
                        .WithMany("Tags")
                        .HasForeignKey("GuildForeignId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}
