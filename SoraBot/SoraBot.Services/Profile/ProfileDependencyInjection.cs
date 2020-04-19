using Microsoft.Extensions.DependencyInjection;

namespace SoraBot.Services.Profile
{
    public static class ProfileDependencyInjection
    {
        public static IServiceCollection AddProfileServices(this IServiceCollection services)
            => services.AddSingleton<IExpService, ExpService>();
    }
}