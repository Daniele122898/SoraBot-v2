using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using SoraBot_v2.Data.Entities;
using SoraBot_v2.Data.Entities.SubEntities;
using SoraBot_v2.Services;
using System.IO;

namespace SoraBot_v2.Data
{
    public class SoraContext : DbContext
    {
        //User Database
        public DbSet<User> Users { get; set; }
        public DbSet<Interactions> Interactions { get; set; }
        public DbSet<Afk> Afk { get; set; }
        public DbSet<Reminders> Reminders { get; set; }
        public DbSet<Marriage> Marriages { get; set; }
        public DbSet<ShareCentral> ShareCentrals { get; set; }
        public DbSet<Voting> Votings { get; set; }

        //Guild Database
        public DbSet<Guild> Guilds { get; set; }
        public DbSet<Tags> Tags { get; set; }
        public DbSet<StarMessage> StarMessages { get; set; }
        public DbSet<Role> SelfAssignableRoles { get; set; }
        public DbSet<ModCase> Cases { get; set; }

        //Song list
        public DbSet<Song> Songs { get; set; }

        //private static volatile object _padlock = new Object();

        /*
        public SoraContext(string con)
        {
            _connectionString =con;
        }*/
        public SoraContext() : base()
        {

        }

        /*
        public SoraContext()
        {
        }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(@"");
        }*/


        //// Added by Catherine Renelle - Memory Leak Fix (also improves migration code)
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string connectionString;

            if (!ConfigService.GetConfig().TryGetValue("connectionString", out connectionString))
            {
                throw new IOException
                {
                    Source = "Couldn't find a \"connectionString\" entry in the config.json file. Exiting."
                };
            }

            optionsBuilder.UseMySql(connectionString);
        }
        ////

        /*public int SaveChangesThreadSafe()
        {
            lock (_padlock)
            {
                try
                {
                    return this.SaveChanges();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    foreach (var entry in ex.Entries)
                    {
                        //REMINDERS
                        if (entry.Entity is Reminders)
                        {
                            var databaseEntity = this.Reminders.AsNoTracking()
                                .Single(x => x.Id == ((Reminders) entry.Entity).Id);
                            var databaseEntry = this.Entry(databaseEntity);

                            foreach (var property in entry.Metadata.GetProperties())
                            {
                                var proposedValue = entry.Property(property.Name).CurrentValue;
                                var originalValue = entry.Property(property.Name).OriginalValue;
                                var databaseValue = entry.Property(property.Name).CurrentValue;

                                // TODO: Logic to decide which value should be written to database
                                // entry.Property(property.Name).CurrentValue = <value to be saved>;

                                entry.Property(property.Name).OriginalValue =
                                    databaseEntry.Property(property.Name).CurrentValue;
                            }

                        }
                        else
                        {
                            throw new NotSupportedException("Don't know how to handle concurrency conflicts for "+ entry.Metadata.Name);
                        }
                    }
                    //retry the save operation
                    return this.SaveChanges();
                }
            }
        }*/

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Interactions>(x =>
            {
                x.HasOne(d => d.User)
                    .WithOne(p => p.Interactions)
                    .HasForeignKey<Interactions>(g => g.UserForeignId);
            });

            modelBuilder.Entity<ShareCentral>(x =>
            {
                x.HasOne(d => d.User)
                    .WithMany(p => p.ShareCentrals)
                    .HasForeignKey(p => p.CreatorId);
            });

            modelBuilder.Entity<Voting>(x =>
            {
                x.HasOne(d => d.User)
                    .WithMany(p => p.Votings)
                    .HasForeignKey(p => p.ShareLink)
                    .HasForeignKey(p => p.VoterId);
            });

            modelBuilder.Entity<Tags>(x =>
            {
                x.HasOne(g => g.Guild)
                    .WithMany(p => p.Tags)
                    .HasForeignKey(g => g.GuildForeignId);
            });

            modelBuilder.Entity<Role>(x =>
            {
                x.HasOne(g => g.Guild)
                    .WithMany(p => p.SelfAssignableRoles)
                    .HasForeignKey(g => g.GuildForeignId);
            });

            modelBuilder.Entity<StarMessage>(x =>
            {
                x.HasOne(g => g.Guild)
                    .WithMany(s => s.StarMessages)
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