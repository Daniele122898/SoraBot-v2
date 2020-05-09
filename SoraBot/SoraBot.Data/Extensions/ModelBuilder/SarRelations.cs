using Microsoft.EntityFrameworkCore;
using SoraBot.Data.Models.SoraDb;

namespace SoraBot.Data.Extensions.ModelBuilder
{
    public static class SarRelations
    {
        public static Microsoft.EntityFrameworkCore.ModelBuilder AddSarRelations(
            this Microsoft.EntityFrameworkCore.ModelBuilder mb)
        {
            mb.Entity<Sar>()
                .HasOne(s => s.Guild)
                .WithMany(g => g.Sars)
                .HasForeignKey(k => k.GuildId)
                .OnDelete(DeleteBehavior.Cascade);

            return mb;
        }
    }
}