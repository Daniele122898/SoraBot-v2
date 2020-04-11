using Microsoft.Extensions.DependencyInjection;
using SoraBot.Common.Extensions.Hosting;

namespace SoraBot.Services.Core
{
    public static class CoreSetupServiceCollectionExtension
    {
        public static IServiceCollection AddSoraBotCore(this IServiceCollection services)
            => services
                .AddSingleton<IBehavior, DiscordSocketCoreListeningBehavior>();
    }
}