using Microsoft.EntityFrameworkCore;
using SoraBot.Data.Models.SoraDb;

namespace SoraBot.Data.Extensions.ModelBuilder
{
    public static class WaifuRelations
    {
        public static Microsoft.EntityFrameworkCore.ModelBuilder AddWaifuRelations(
            this Microsoft.EntityFrameworkCore.ModelBuilder mb)
        {
            mb.Entity<UserWaifu>()
                .HasKey(k => new {k.UserId, k.WaifuId});

            mb.Entity<UserWaifu>()
                .HasOne(u => u.Owner)
                .WithMany(w => w.UserWaifus)
                .HasForeignKey(k => k.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            mb.Entity<UserWaifu>()
                .HasOne(w => w.Waifu)
                .WithMany(m => m.UserWaifus)
                .HasForeignKey(k => k.WaifuId)
                .OnDelete(DeleteBehavior.Cascade);

            return mb;
        }
    }
}