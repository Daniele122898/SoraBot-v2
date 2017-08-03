using System;
using Microsoft.EntityFrameworkCore;
using SoraBot_v2.Data.Entities;
using SoraBot_v2.Data.Entities.SubEntities;

namespace SoraBot_v2.Data
{
    public class SoraContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Interactions> Interactions { get; set; }

        private string _connectionString;

        public SoraContext(string con)
        {
            _connectionString = con;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(@""+_connectionString);
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