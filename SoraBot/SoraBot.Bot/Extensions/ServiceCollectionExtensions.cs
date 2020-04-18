using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SoraBot.Common.Extensions.Hosting;
using SoraBot.Data.Configurations;
using SoraBot.Services.Cache;
using SoraBot.Services.Core;
using SoraBot.Services.Guilds;
using SoraBot.Services.Users;
using SoraBot.Services.Utils;
using SoraBot.Services.Waifu;

namespace SoraBot.Bot.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSoraBot(this IServiceCollection services)
        {

            services.AddSingleton(
                provider => new DiscordSocketClient(new DiscordSocketConfig()
                {
                    AlwaysDownloadUsers = false,
                    LogLevel = LogSeverity.Debug,
                    MessageCacheSize = 0, // Let's have this disabled for now. 
                    TotalShards = provider.GetService<IOptions<SoraBotConfig>>().Value.TotalShards,
                    ShardId = 0 // TODO make this configurable
                }));

            services.AddSingleton(new DiscordRestClient(new DiscordRestConfig()
            {
                LogLevel = LogSeverity.Debug
            }));

            services.AddSingleton(_ =>
            {
                var service = new CommandService(new CommandServiceConfig()
                {
                    LogLevel = LogSeverity.Debug,
                    DefaultRunMode = RunMode.Sync,
                    CaseSensitiveCommands = false,
                    SeparatorChar = ' '
                });
                // Here i could add type readers or programatically added commands etc
                return service;
            });

            services.AddSingleton<DiscordSerilogAdapter>();
            services.AddSingleton<InteractiveService>();

            services.AddSingleton<IHostedService, BehaviorHost>()
                .AddSoraBotCore()
                .AddSoraMessaging()
                .AddCacheService()
                .AddWaifuServices()
                .AddGuildServices()
                .AddUtilServices()
                .AddUserServices();

            services.AddHostedService<SoraBot>();
            
            return services;
        }
    }
}