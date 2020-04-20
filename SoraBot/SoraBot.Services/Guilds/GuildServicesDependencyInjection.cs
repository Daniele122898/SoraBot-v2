using Microsoft.Extensions.DependencyInjection;

namespace SoraBot.Services.Guilds
{
    public static class GuildServicesDependencyInjection
    {
        public static IServiceCollection AddGuildServices(this IServiceCollection services)
            => services.AddScoped<IPrefixService, PrefixService>();
    }
}