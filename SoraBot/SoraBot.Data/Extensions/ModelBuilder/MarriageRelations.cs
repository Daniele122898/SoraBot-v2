using System;
using Microsoft.EntityFrameworkCore;
using SoraBot.Data.Models.SoraDb;

namespace SoraBot.Data.Extensions.ModelBuilder
{
    public static class MarriageRelations
    {
        public static Microsoft.EntityFrameworkCore.ModelBuilder AddMarriageRelations(
            this Microsoft.EntityFrameworkCore.ModelBuilder mb)
        {
            mb.Entity<Marriage>()
                .HasKey(k => new {k.Partner1, k.Partner2});

            mb.Entity<Marriage>()
                .HasOne(m => m.Partner1User)
                .WithMany(u => u.Marriages)
                .HasForeignKey(k => k.Partner1)
                .OnDelete(DeleteBehavior.Cascade);
            
            mb.Entity<Marriage>()
                .HasOne(m => m.Partner2User)
                .WithMany(u => u.Marriages)
                .HasForeignKey(k => k.Partner2)
                .OnDelete(DeleteBehavior.Cascade);

            mb.Entity<Marriage>()
                .Property(p => p.PartnerSince)
                .IsRequired(true)
                .HasDefaultValue(DateTime.UtcNow);

            return mb;
        }
    }
}