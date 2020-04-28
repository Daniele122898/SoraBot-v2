using Microsoft.Extensions.DependencyInjection;

namespace SoraBot.Services.ReactionHandlers
{
    public static class ReactionHandlerDependencyInjection
    {
        public static IServiceCollection AddReactionHandlerServices(this IServiceCollection services)
            => services;
    }
}