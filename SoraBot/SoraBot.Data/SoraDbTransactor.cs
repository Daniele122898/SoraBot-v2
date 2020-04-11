using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SoraBot.Data.Configurations;

namespace SoraBot.Data
{
    public class SoraDbTransactor : TransactorBase<SoraContext>
    {
        private string _connectionString;

        public SoraDbTransactor(ILogger<TransactorBase<SoraContext>> logger, IOptions<SoraBotConfig> config) : base(logger)
        {
            _connectionString = config.Value?.DbConnection ?? throw new ArgumentNullException(nameof(config));
        }

        protected override SoraContext CreateContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<SoraContext>();
            optionsBuilder.UseLazyLoadingProxies();
            optionsBuilder.UseMySql(_connectionString);
            var context = new SoraContext(optionsBuilder.Options);
            return context;
        }
    }
}