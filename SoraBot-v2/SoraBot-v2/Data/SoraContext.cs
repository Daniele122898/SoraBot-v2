using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using SoraBot_v2.Data.Entities;
using SoraBot_v2.Data.Entities.SubEntities;

namespace SoraBot_v2.Data
{
    public class SoraContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Interactions> Interactions { get; set; }
        public DbSet<Afk> Afk { get; set; }

        private string _connectionString;

        public SoraContext(string con)
        {
            _connectionString = con;
        }
        
        public SoraContext()
        {
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(@""+_connectionString);
            //optionsBuilder.UseMySql(@"");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Interactions>(x =>
            {
                x.HasOne(d => d.User)
                    .WithOne(p => p.Interactions)
                    .HasForeignKey<Interactions>(g => g.UserForeignId);
            });
        }
    }
}