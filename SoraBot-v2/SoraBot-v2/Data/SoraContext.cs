using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using SoraBot_v2.Data.Entities;
using SoraBot_v2.Data.Entities.SubEntities;

namespace SoraBot_v2.Data
{
    public class SoraContext : DbContext
    {
        //User Database
        public DbSet<User> Users { get; set; }
        public DbSet<Interactions> Interactions { get; set; }
        public DbSet<Afk> Afk { get; set; }
        public DbSet<Reminders> Reminders { get; set; }
        public DbSet<Marriage> Marriages{ get; set; }
        
        //Guild Database
        public DbSet<Guild> Guilds { get; set; }
        public DbSet<Tags> Tags { get; set; }
        
        //Song list
        public DbSet<Song> Songs { get; set; }
        
        private string _connectionString;

        private static volatile object _padlock = new Object();

        public SoraContext(string con)
        {
            _connectionString =con;
        }
        
        public SoraContext()
        {
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(@""+_connectionString);
            //optionsBuilder.UseMySql(@"");
        }

        public int SaveChangesThreadSafe()
        {
            lock (_padlock)
            {
                return SaveChanges();
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Interactions>(x =>
            {
                x.HasOne(d => d.User)
                    .WithOne(p => p.Interactions)
                    .HasForeignKey<Interactions>(g => g.UserForeignId);
            });

            modelBuilder.Entity<Tags>(x =>
            {
                x.HasOne(g => g.Guild)
                    .WithMany(p => p.Tags)
                    .HasForeignKey(g => g.GuildForeignId);
            });

            modelBuilder.Entity<Reminders>(x =>
            {
                x.HasOne(g => g.User)
                    .WithMany(p => p.Reminders)
                    .HasForeignKey(g => g.UserForeignId);
            });
            
            modelBuilder.Entity<Marriage>(x =>
            {
                x.HasOne(g => g.User)
                    .WithMany(p => p.Marriages)
                    .HasForeignKey(g => g.UserForeignId);
            });
        }
    }
}