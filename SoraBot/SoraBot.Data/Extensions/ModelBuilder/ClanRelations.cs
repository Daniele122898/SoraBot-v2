using Microsoft.EntityFrameworkCore;
using SoraBot.Data.Models.SoraDb;

namespace SoraBot.Data.Extensions.ModelBuilder
{
    public static class ClanRelations
    {
        public static Microsoft.EntityFrameworkCore.ModelBuilder AddClanRelations(this Microsoft.EntityFrameworkCore.ModelBuilder mb)
        {
            mb.Entity<Clan>()
                .Property(p => p.Description)
                .IsRequired(false);
            
            mb.Entity<Clan>()
                .Property(p => p.AvatarUrl)
                .IsRequired(false);

            mb.Entity<Clan>()
                .Property(p => p.Level)
                .IsRequired(true)
                .HasDefaultValue(0);

            // Make name unique
            mb.Entity<Clan>()
                .HasAlternateKey(k => k.Name);

            mb.Entity<Clan>()
                .HasOne(u => u.Owner)
                .WithOne(a => a.ClanOwner)
                .HasForeignKey<Clan>(k => k.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);

            mb.Entity<ClanMember>()
                .HasKey(k => new {k.ClanId, k.UserId});

            mb.Entity<ClanMember>()
                .HasOne(u => u.Clan)
                .WithMany(m => m.Members)
                .HasForeignKey(k => k.ClanId)
                .OnDelete(DeleteBehavior.Cascade);

            mb.Entity<ClanMember>()
                .HasOne(u => u.User)
                .WithOne(c => c.ClanMember)
                .HasForeignKey<ClanMember>(k => k.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            mb.Entity<ClanInvite>()
                .HasKey(k => new {k.ClanId, k.UserId});

            mb.Entity<ClanInvite>()
                .HasOne(u => u.Clan)
                .WithMany(m => m.Invites)
                .HasForeignKey(k => k.ClanId)
                .OnDelete(DeleteBehavior.Cascade);

            mb.Entity<ClanInvite>()
                .HasOne(u => u.User)
                .WithMany(c => c.ClanInvites)
                .HasForeignKey(k => k.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            return mb;
        }
    }
}