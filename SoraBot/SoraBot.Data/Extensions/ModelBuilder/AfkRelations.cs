using Microsoft.EntityFrameworkCore;
using SoraBot.Data.Models.SoraDb;

namespace SoraBot.Data.Extensions.ModelBuilder
{
    public static class AfkRelations
    {
        public static Microsoft.EntityFrameworkCore.ModelBuilder AddAfkRelations(this Microsoft.EntityFrameworkCore.ModelBuilder mb)
        {
            mb.Entity<Afk>()
                .HasKey(k => k.UserId);

            mb.Entity<Afk>()
                .HasOne(u => u.User)
                .WithOne(a => a.Afk)
                .HasForeignKey<Afk>(k => k.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            return mb;
        }
    }
}