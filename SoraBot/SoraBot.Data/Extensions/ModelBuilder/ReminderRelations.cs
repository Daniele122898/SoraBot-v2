using Microsoft.EntityFrameworkCore;
using SoraBot.Data.Models.SoraDb;

namespace SoraBot.Data.Extensions.ModelBuilder
{
    public static class ReminderRelations
    {
        public static Microsoft.EntityFrameworkCore.ModelBuilder AddReminderRelations(
            this Microsoft.EntityFrameworkCore.ModelBuilder mb)
        {
            mb.Entity<Reminder>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reminders)
                .HasForeignKey(k => k.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            return mb;
        }
    }
}