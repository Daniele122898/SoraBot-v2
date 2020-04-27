using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SoraBot.Bot.Models;
using SoraBot.Bot.TypeReaders;
using SoraBot.Common.Extensions.Hosting;
using SoraBot.Data.Configurations;
using SoraBot.Services.Cache;
using SoraBot.Services.Core;
using SoraBot.Services.Guilds;
using SoraBot.Services.Misc;
using SoraBot.Services.Profile;
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
                    ShardId = GlobalConstants.ShardId,
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
                // Adding custom IUser and IGuildUser type readers bcs the default ones suck 
                service.AddTypeReader<DiscordUser>(new UserTypeReader(), true);
                service.AddTypeReader<DiscordGuildUser>(new GuildUserTypeReader(), true);
                
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
                .AddUserServices()
                .AddProfileServices()
                .AddMiscServices();

            services.AddHostedService<SoraBot>();
            
            return services;
        }
    }
}