using Microsoft.Extensions.DependencyInjection;

namespace SoraBot.Services.Cache
{
    public static class CacheServiceDependencyInjection
    {
        public static IServiceCollection AddCacheService(this IServiceCollection services)
        {
            services.AddSingleton<ICacheService, CacheService>();
            
            return services;
        }
    }
}