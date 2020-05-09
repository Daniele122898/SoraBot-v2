using Microsoft.EntityFrameworkCore;
using SoraBot.Data.Extensions.ModelBuilder;
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
        public DbSet<Reminder> Reminders { get; set; }
        
        public DbSet<Waifu> Waifus { get; set; }
        public DbSet<UserWaifu> UserWaifus { get; set; }

        public DbSet<Guild> Guilds { get; set; }
        public DbSet<GuildUser> GuildUsers { get; set; }
        public DbSet<Sar> Sars { get; set; }
        
        public DbSet<Starboard> Starboards { get; set; }
        public DbSet<StarboardMessage> StarboardMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.AddWaifuRelations();
            builder.AddStarboardRelations();
            builder.AddGuildRelations();
            builder.AddReminderRelations();
            builder.AddSarRelations();
        }
    }
}