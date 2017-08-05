using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using SoraBot_v2.Data;
using SoraBot_v2.Services;

namespace SoraBot_v2
{
    public class CommandHandler
    {
        private IServiceProvider _services;
        private DiscordSocketClient _client;
        private CommandService _commands;
        private AfkService _afkService;
        private SoraContext _soraContext;

        public CommandHandler(IServiceProvider services)
        {
            _client = services.GetService<DiscordSocketClient>();
            _commands = services.GetService<CommandService>();
            _afkService = services.GetService<AfkService>();
            _soraContext = services.GetService<SoraContext>();
            _services = services;
        }

        public async Task InstallAsync()
        {
            _client.MessageReceived += HandleCommandsAsync;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        public async Task HandleCommandsAsync(SocketMessage m)
        {
            var message = m as SocketUserMessage;
            if (message == null) return;
            if (message.Author.IsBot) return;
            if (!(message.Channel is SocketGuildChannel)) return;
            
            //Hand to AFK service
            await _afkService.Client_MessageReceived(m, _soraContext);
            
            //prefix ends and command starts
            string prefix = ">";
            int argPos = prefix.Length-1;
            if(!(message.HasStringPrefix(prefix, ref argPos)|| message.HasMentionPrefix(_client.CurrentUser, ref argPos)))
                return;
            
            var context = new SocketCommandContext(_client,message);

            var result = await _commands.ExecuteAsync(context, argPos, _services);

            if (!result.IsSuccess)
            {
                await context.Channel.SendMessageAsync($"**FAILED**\n{result.ErrorReason}");
            }
        }
    }
}