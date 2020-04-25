﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SoraBot.Data;

namespace SoraBot.Data.Migrations
{
    [DbContext(typeof(SoraContext))]
    partial class SoraContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.3")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("SoraBot.Data.Models.SoraDb.Guild", b =>
                {
                    b.Property<ulong>("Id")
                        .HasColumnType("bigint unsigned");

                    b.Property<string>("Prefix")
                        .IsRequired()
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.HasKey("Id");

                    b.ToTable("Guilds");
                });

            modelBuilder.Entity("SoraBot.Data.Models.SoraDb.User", b =>
                {
                    b.Property<ulong>("Id")
                        .HasColumnType("bigint unsigned");

                    b.Property<uint>("Coins")
                        .HasColumnType("int unsigned");

                    b.Property<uint>("Exp")
                        .HasColumnType("int unsigned");

                    b.Property<int?>("FavoriteWaifuId")
                        .HasColumnType("int");

                    b.Property<bool>("HasCustomProfileBg")
                        .HasColumnType("tinyint(1)");

                    b.Property<DateTime>("LastDaily")
                        .HasColumnType("datetime(6)");

                    b.HasKey("Id");

                    b.HasIndex("FavoriteWaifuId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("SoraBot.Data.Models.SoraDb.UserWaifu", b =>
                {
                    b.Property<ulong>("UserId")
                        .HasColumnType("bigint unsigned");

                    b.Property<int>("WaifuId")
                        .HasColumnType("int");

                    b.Property<uint>("Count")
                        .HasColumnType("int unsigned");

                    b.HasKey("UserId", "WaifuId");

                    b.HasIndex("WaifuId");

                    b.ToTable("UserWaifus");
                });

            modelBuilder.Entity("SoraBot.Data.Models.SoraDb.Waifu", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("ImageUrl")
                        .IsRequired()
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<int>("Rarity")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("Waifus");
                });

            modelBuilder.Entity("SoraBot.Data.Models.SoraDb.User", b =>
                {
                    b.HasOne("SoraBot.Data.Models.SoraDb.Waifu", "FavoriteWaifu")
                        .WithMany("UsersFavorite")
                        .HasForeignKey("FavoriteWaifuId")
                        .OnDelete(DeleteBehavior.SetNull);
                });

            modelBuilder.Entity("SoraBot.Data.Models.SoraDb.UserWaifu", b =>
                {
                    b.HasOne("SoraBot.Data.Models.SoraDb.User", "Owner")
                        .WithMany("UserWaifus")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("SoraBot.Data.Models.SoraDb.Waifu", "Waifu")
                        .WithMany("UserWaifus")
                        .HasForeignKey("WaifuId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
