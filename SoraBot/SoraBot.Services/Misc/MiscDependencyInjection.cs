using Microsoft.Extensions.DependencyInjection;

namespace SoraBot.Services.Misc
{
    public static class MiscDependencyInjection
    {
        public static IServiceCollection AddMiscServices(this IServiceCollection services)
            => services.AddSingleton<WeebService>();
    }
}