using Microsoft.EntityFrameworkCore;
using SoraBot.Data.Models.SoraDb;

namespace SoraBot.Data.Extensions.ModelBuilder
{
    public static class StarboardRelations
    {
        public static Microsoft.EntityFrameworkCore.ModelBuilder AddStarboardRelations(
            this Microsoft.EntityFrameworkCore.ModelBuilder mb)
        {
            mb.Entity<Starboard>()
                .Property(p => p.StarboardThreshold)
                .HasDefaultValue(1);

            mb.Entity<Starboard>()
                .HasOne(s => s.Guild)
                .WithOne(g => g.Starboard)
                .HasForeignKey<Starboard>(k => k.GuildId)
                .IsRequired(true)
                .OnDelete(DeleteBehavior.Cascade);

            mb.Entity<Starboard>()
                .HasKey(k => k.GuildId);

            return mb;
        }
    }
}