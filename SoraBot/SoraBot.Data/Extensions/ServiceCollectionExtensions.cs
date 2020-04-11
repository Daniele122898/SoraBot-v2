using Microsoft.Extensions.DependencyInjection;

namespace SoraBot.Data.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSoraData(this IServiceCollection services)
        {
            services.AddScoped<ITransactor<SoraContext>, SoraDbTransactor>();
            
            return services;
        }
    }
}