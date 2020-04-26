using Microsoft.Extensions.DependencyInjection;

namespace SoraBot.Services.Utils
{
    public static class UtilsServicesDependencyInjection
    {
        public static IServiceCollection AddUtilServices(this IServiceCollection services)
        {
            // Don't want to create a Interface for smth this simple. Seems stupidly overkill
            services.AddSingleton<RandomNumberService>();
            services.AddSingleton<HttpClientHelper>();
            services.AddSingleton<HealthChecker>();
            
            return services;
        }
    }
}