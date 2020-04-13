using Microsoft.EntityFrameworkCore;
using SoraBot.Data.Models.SoraDb;

namespace SoraBot.Data
{
    public class SoraContext : DbContext
    {
        public SoraContext(DbContextOptions<SoraContext> options) : base(options)
        {
        }

        // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        // {
        //     base.OnConfiguring(optionsBuilder);
        //     optionsBuilder.UseLazyLoadingProxies();
        // }

        public DbSet<User> Users { get; set; }
        public DbSet<Waifu> Waifus { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
        }
    }
}