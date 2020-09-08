using Microsoft.Extensions.DependencyInjection;

namespace SoraBot.Services.Afk
{
    public static class AfkDependencyInjection
    {
        public static IServiceCollection AddAfkServices(this IServiceCollection services)
            => services.AddSingleton<IAfkService, AfkService>();
    }
}