using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using SoraBot_v2.Data;

namespace SoraBotv2.Migrations
{
    [DbContext(typeof(SoraContext))]
    partial class SoraContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.2");

            modelBuilder.Entity("SoraBot_v2.Data.Entities.SubEntities.Interactions", b =>
                {
                    b.Property<int>("InteractionsId")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("Hugs");

                    b.Property<int>("Kisses");

                    b.Property<int>("Pats");

                    b.Property<int>("Pokes");

                    b.Property<int>("Punches");

                    b.Property<int>("Slaps");

                    b.Property<ulong>("UserForeignId");

                    b.HasKey("InteractionsId");

                    b.HasIndex("UserForeignId")
                        .IsUnique();

                    b.ToTable("Interactions");
                });

            modelBuilder.Entity("SoraBot_v2.Data.Entities.User", b =>
                {
                    b.Property<ulong>("UserId");

                    b.HasKey("UserId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("SoraBot_v2.Data.Entities.SubEntities.Interactions", b =>
                {
                    b.HasOne("SoraBot_v2.Data.Entities.User", "User")
                        .WithOne("Interactions")
                        .HasForeignKey("SoraBot_v2.Data.Entities.SubEntities.Interactions", "UserForeignId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}
