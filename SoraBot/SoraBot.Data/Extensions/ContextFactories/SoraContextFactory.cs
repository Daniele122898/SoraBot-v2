using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SoraBot.Data.Extensions.ContextFactories
{
    public class SoraContextFactory : IDesignTimeDbContextFactory<SoraContext>
    {
        
        public SoraContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "../SoraBot.WebApi", "appsettings.Development.json"))
                .Build();

            var connectionString = configuration.GetSection("SoraBotSettings").GetValue<string>("DbConnection");
            
            var optionsBuilder = new DbContextOptionsBuilder<SoraContext>();
            optionsBuilder.UseLazyLoadingProxies();
            optionsBuilder.UseMySql(connectionString);
            var context = new SoraContext(optionsBuilder.Options);
            return context;
        }
    }
}