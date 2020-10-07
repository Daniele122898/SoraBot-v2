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
                .HasKey(k => new {Partner1 = k.Partner1Id, Partner2 = k.Partner2Id});

            mb.Entity<Marriage>()
                .Property(p => p.PartnerSince)
                .IsRequired(true)
                .HasDefaultValue(DateTime.UtcNow);

            return mb;
        }
    }
}