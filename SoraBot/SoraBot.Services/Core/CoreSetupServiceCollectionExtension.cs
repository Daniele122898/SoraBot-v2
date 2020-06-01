using Microsoft.Extensions.DependencyInjection;
using SoraBot.Common.Extensions.Hosting;
using SoraBot.Common.Messages;
using SoraBot.Common.Messages.MessageAdapters;
using SoraBot.Services.Core.MessageHandlers;

namespace SoraBot.Services.Core
{
    public static class CoreSetupServiceCollectionExtension
    {
        public static IServiceCollection AddSoraBotCore(this IServiceCollection services)
            => services
                .AddSingleton<IBehavior, DiscordSocketCoreListeningBehavior>();

        public static IServiceCollection AddSoraMessaging(this IServiceCollection services)
            => services
                .AddSingleton<IMessageBroker, MessageBroker>()
                .AddScoped<IMessageHandler<MessageReceived>, MessageReceivedHandler>()
                .AddScoped<IMessageHandler<MessageReceived>, MessageEventHandler>()
                .AddScoped<IMessageHandler<ReactionReceived>, ReactionReceivedHandler>()
                .AddScoped<IMessageHandler<UserJoined>, UserJoinLeaveEventHandler>()
                .AddScoped<IMessageHandler<UserLeft>, UserJoinLeaveEventHandler>();
    }
}