using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SoraBot.Data.Configurations;

namespace SoraBot.WebApi.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddConfigurations(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<SoraBotConfig>(configuration.GetSection("SoraBotSettings"));
            services.Configure<LavaLinkConfig>(configuration.GetSection("LavaLink"));
            services.Configure<ApiConfig>(configuration.GetSection("ApiSettings"));
            
            return services;
        }
    }
}