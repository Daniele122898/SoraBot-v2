using Microsoft.EntityFrameworkCore;
using SoraBot.Data.Models.SoraDb;

namespace SoraBot.Data.Extensions.ModelBuilder
{
    public static class WaifuRequestRelations
    {
        public static Microsoft.EntityFrameworkCore.ModelBuilder AddWaifuRequestRelations(
            this Microsoft.EntityFrameworkCore.ModelBuilder mb)
        {
            mb.Entity<WaifuRequest>()
                .HasOne(k => k.User)
                .WithMany(u => u.WaifuRequests)
                .HasForeignKey(k => k.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            mb.Entity<WaifuRequest>()
                .Property(p => p.ProcessedTime)
                .IsRequired(false)
                .HasDefaultValue(null);

            mb.Entity<WaifuRequest>()
                .Property(p => p.RejectReason)
                .IsRequired(false)
                .HasDefaultValue(null);
            
            mb.Entity<WaifuRequest>()
                .Property(p => p.RequestState)
                .HasDefaultValue(RequestState.Pending);

            
            return mb;
        }
    }
}