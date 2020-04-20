using Microsoft.Extensions.DependencyInjection;

namespace SoraBot.Services.Users
{
    public static class UserServicesDependencyInjection
    {
        public static IServiceCollection AddUserServices(this IServiceCollection services)
            => services.AddScoped<IUserService, UserService>();
    }
}