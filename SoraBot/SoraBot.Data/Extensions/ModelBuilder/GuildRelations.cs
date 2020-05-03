using Microsoft.EntityFrameworkCore;
using SoraBot.Data.Models.SoraDb;

namespace SoraBot.Data.Extensions.ModelBuilder
{
    public static class GuildRelations
    {
        public static Microsoft.EntityFrameworkCore.ModelBuilder AddGuildRelations(
            this Microsoft.EntityFrameworkCore.ModelBuilder mb)
        {
            mb.Entity<GuildUser>()
                .HasKey(k => new {k.GuildId, k.UserId});

            mb.Entity<GuildUser>()
                .HasOne(g => g.Guild)
                .WithMany(g => g.GuildUsers)
                .HasForeignKey(x => x.GuildId)
                .OnDelete(DeleteBehavior.Cascade);

            mb.Entity<GuildUser>()
                .HasOne(g => g.User)
                .WithMany(u => u.GuildUsers)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            return mb;
        }
    }
}