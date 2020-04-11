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

            services.AddDbContextPool<SoraContext>(op =>
            {
                op.UseLazyLoadingProxies();
                op.UseMySql(configs.GetSection("SoraBotSettings").GetValue<string>("DbConnection"));
            });
            
            return services;
        }
    }
}