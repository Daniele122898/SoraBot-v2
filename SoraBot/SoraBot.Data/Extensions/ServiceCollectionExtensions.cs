using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace SoraBot.Data.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSoraData(this IServiceCollection services, IConfiguration configs)
        {
            services.AddScoped<ITransactor<SoraContext>, SoraDbTransactor>();
            // Use this pool in the transactor as well for improved performance
            services.AddDbContextPool<SoraContext>(op =>
            {
                op.UseLazyLoadingProxies();
                op.UseMySql(configs.GetSection("SoraBotSettings").GetValue<string>("DbConnection"));
            });

            // Inject the context factory so we use the ContextPool
            services.AddScoped<Func<SoraContext>>(provider => provider.GetRequiredService<SoraContext>);
            
            
            return services;
        }
    }
}