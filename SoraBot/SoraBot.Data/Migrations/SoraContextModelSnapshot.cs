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

            modelBuilder.Entity("SoraBot.Data.Models.SoraDb.User", b =>
                {
                    b.Property<ulong>("Id")
                        .HasColumnType("bigint unsigned");

                    b.Property<uint>("Coins")
                        .HasColumnType("int unsigned");

                    b.Property<DateTime>("LastDaily")
                        .HasColumnType("datetime(6)");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });
#pragma warning restore 612, 618
        }
    }
}