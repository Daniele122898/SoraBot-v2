using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using SoraBot_v2.Data;
using SoraBot_v2.Services;

namespace SoraBot_v2
{
    public class CommandHandler
    {
        public double MessagesReceived;
        public int CommandsExecuted;
        
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

        public CommandHandler()
        {
            
        }

        public void ConfigureCommandHandler(IServiceProvider services)
        {
            _client = services.GetService<DiscordSocketClient>();
            _commands = services.GetService<CommandService>();
            _afkService = services.GetService<AfkService>();
            _soraContext = services.GetService<SoraContext>();
            _services = services;
            
            _commands.Log += CommandsOnLog;
        }

        private Task CommandsOnLog(LogMessage logMessage)
        {
            return HandleErrorAsync(logMessage);
        }

        public async Task InstallAsync()
        {
            _client.MessageReceived += HandleCommandsAsync;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        public async Task HandleCommandsAsync(SocketMessage m)
        {
            try
            {
                MessagesReceived++;
                var message = m as SocketUserMessage;
                if (message == null) return;
                if (message.Author.IsBot) return;
                if (!(message.Channel is SocketGuildChannel)) return;
            
                //Hand to AFK service
                await _afkService.Client_MessageReceived(m, _soraContext);
            
                //create Context
                var context = new SocketCommandContext(_client,message);
            
                //prefix ends and command starts
                string prefix = Utility.GetGuildPrefix(context.Guild, _soraContext);
                int argPos = prefix.Length-1;
                if(!(message.HasStringPrefix(prefix, ref argPos)|| message.HasMentionPrefix(_client.CurrentUser, ref argPos)))
                    return;
            
            

                var result = await _commands.ExecuteAsync(context, argPos, _services);

                if (!result.IsSuccess)
                {
                    //await context.Channel.SendMessageAsync($"**FAILED**\n{result.ErrorReason}");
                    await HandleErrorAsync(result, context);
                }
                else if (result.IsSuccess)
                    CommandsExecuted++;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private async Task HandleErrorAsync(IResult result, SocketCommandContext context, CommandException exception = null)
        {
            switch (result.Error)
            {
                    case CommandError.Exception:
                        if (exception != null)
                        {
                            await context.Channel.SendMessageAsync(
                                $"**Exception**\n{exception.InnerException.Message}\n```\n{exception.InnerException.StackTrace}```");
                        }
                        break;
                    case CommandError.BadArgCount:
                        await context.Channel.SendMessageAsync($"**FAILED**\n{result.ErrorReason}");
                        break;        
                    default:
                        await context.Channel.SendMessageAsync($"**FAILED**\n{result.ErrorReason}");
                        break;
            }
        }

        private async Task HandleErrorAsync(LogMessage logMessage)
        {
            var commandException = logMessage.Exception as CommandException;
            if(commandException == null) return;
            await HandleErrorAsync(ExecuteResult.FromError(commandException),
                (SocketCommandContext) commandException.Context, commandException);
        }
    }
}