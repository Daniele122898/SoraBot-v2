using Microsoft.Extensions.DependencyInjection;

namespace SoraBot.Bot.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSoraBot(this IServiceCollection services)
        {

            services.AddHostedService<SoraBot>();
            
            return services;
        }
    }
}