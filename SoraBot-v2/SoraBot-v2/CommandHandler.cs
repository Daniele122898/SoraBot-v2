using System;
using System.Dynamic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Net;
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
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly AfkService _afkService;
        private EpService _epService;
        private StarboardService _starboardService;
        private readonly RatelimitingService _ratelimitingService;
        private SelfAssignableRolesService _selfAssignableRolesService;
        private AnnouncementService _announcementService;
        private ModService _modService;
        private readonly GuildCountUpdaterService _guildCount;

        private async Task ClientOnJoinedGuild(SocketGuild socketGuild)
        {
            //Notify discordbots that we joined a new guild :P
            try
            {
                await _guildCount.UpdateCount(_client.Guilds.Count);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            using (var soraContext = _services.GetService<SoraContext>())
            {
                var guild = Utility.GetOrCreateGuild(socketGuild.Id, soraContext);
                //AUTO CREATE SORA ADMIN ROLE
                //var created = await Utility.CreateSoraRoleOnJoinAsync(socketGuild, _client, soraContext).ConfigureAwait(false);
                /*
                if (created)
                {
                    guild.RestrictTags = socketGuild.MemberCount > 100;
                }*/
                await soraContext.SaveChangesAsync();
                try
                {
                    string prefix = Utility.GetGuildPrefix(socketGuild, soraContext);
                    await (await socketGuild.Owner.GetOrCreateDMChannelAsync()).SendMessageAsync("", embed: Utility.ResultFeedback(Utility.BlueInfoEmbed, Utility.SuccessLevelEmoji[3], $"Hello there (≧∇≦)/")
                        .WithDescription($"I'm glad you invited me over :)\n" +
                                         $"You can find the [list of commands and help here](http://git.argus.moe/serenity/SoraBot-v2/wikis/home)\n"+
                                         $"To restrict tag creation and Sora's mod functions you must create\n" +
                                         $"a {Utility.SORA_ADMIN_ROLE_NAME} Role so that only the ones carrying it can create\n" +
                                         $"tags or use Sora's mod functionality. You can make him create one with: "+
                                         $"`{prefix}createAdmin`\n" +
                                         $"You can leave tag creation unrestricted if you want but its not\n" +
                                         $"recommended on larger servers as it will be spammed.\n" +
                                         $"PS: Standard Prefix is `$` but you can change it with:\n" +
                                         $"`@Sora prefix yourPrefix`\n").WithThumbnailUrl(socketGuild.IconUrl ?? Utility.StandardDiscordAvatar).AddField("Support", $"You can find the [support guild here]({Utility.DISCORD_INVITE})"));
            
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            //inform me of joining
            await SentryService.SendMessage($"**JOINED GUILD**\nName: {socketGuild.Name}\nID: {socketGuild.Id}\nUsers: {socketGuild.MemberCount}\nOwner: {Utility.GiveUsernameDiscrimComb(socketGuild.Owner)}");
            //TODO WELCOME MESSAGE
        }

        public CommandHandler(IServiceProvider provider, DiscordSocketClient client, CommandService commandService,EpService epService, 
            AfkService afkService, RatelimitingService ratelimitingService, StarboardService starboardService, SelfAssignableRolesService selfService, AnnouncementService announcementService,
            ModService modService, GuildCountUpdaterService guildCountUpdaterService)
        {
            _client = client;
            _commands = commandService;
            _afkService = afkService;
            _epService = epService;
            _services = provider;
            _ratelimitingService = ratelimitingService;
            _starboardService = starboardService;
            _selfAssignableRolesService = selfService;
            _announcementService = announcementService;
            _modService = modService;
            _guildCount = guildCountUpdaterService;
            
            _client.MessageReceived += HandleCommandsAsync;
            _commands.Log += CommandsOnLog;
            _client.JoinedGuild += ClientOnJoinedGuild;
            _client.LeftGuild += ClientOnLeftGuild;
            _client.MessageReceived += _epService.IncreaseEpOnMessageReceive;
            _client.ReactionAdded += _starboardService.ClientOnReactionAdded;
            _client.ReactionRemoved += _starboardService.ClientOnReactionRemoved;
            _client.UserJoined += _selfAssignableRolesService.ClientOnUserJoined;
            _client.UserJoined += _announcementService.ClientOnUserJoined;
            _client.UserLeft += _announcementService.ClientOnUserLeft;
            
            //mod Service
            _client.UserBanned += _modService.ClientOnUserBanned;
            _client.UserUnbanned += _modService.ClientOnUserUnbanned;
            
            //count 
            _guildCount.UpdateCount(_client.Guilds.Count);
        }

        private async Task ClientOnLeftGuild(SocketGuild socketGuild)
        {
            //notify discordbots
            try
            {
                await _guildCount.UpdateCount(_client.Guilds.Count);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            await SentryService.SendMessage($"**LEFT GUILD**\nName: {socketGuild.Name}\nID: {socketGuild.Id}\nUsers: {socketGuild.MemberCount}\nOwner: {Utility.GiveUsernameDiscrimComb(socketGuild.Owner)}");
        }

        public async Task InitializeAsync(IServiceProvider provider)
        {
            _services = provider;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        private Task CommandsOnLog(LogMessage logMessage)
        {
            return HandleErrorAsync(logMessage);
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
                using (var soraContext = _services.GetService<SoraContext>())
                {
                    //create Context
                    var context = new SocketCommandContext(_client,message);
                    //Check essential perms, set send message to false here to prevent spam from normal failiure. 
                    //darwinism :P
                    if(await Utility.CheckReadWritePerms(context.Guild, (IGuildChannel)context.Channel, false) == false)
                        return;
                    
                    //Hand to AFK service
                    await _afkService.Client_MessageReceived(m, soraContext);
                
                    //prefix ends and command starts
                    string prefix = "";//Utility.GetGuildPrefix(context.Guild, _soraContext);
                    
                   
                    prefix = Utility.GetGuildPrefix(context.Guild, soraContext);
                    
                    int argPos = prefix.Length-1;
                    if(!(message.HasStringPrefix(prefix, ref argPos)|| message.HasMentionPrefix(_client.CurrentUser, ref argPos)))
                        return;
                
                    //Check ratelimit
                    if(await _ratelimitingService.IsRatelimited(message.Author.Id))
                        return;
                    
                    var result = await _commands.ExecuteAsync(context, argPos, _services);
                    //LOG
                    Logger.WriteRamLog(context);
                    if (!result.IsSuccess)
                    {
                        //await context.Channel.SendMessageAsync($"**FAILED**\n{result.ErrorReason}");
                        await HandleErrorAsync(result, context);
                    }
                    else if (result.IsSuccess)
                    {
                        CommandsExecuted++;
                        _ratelimitingService.RateLimitMain(context.User.Id);
                    }
                }
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
                            await SentryService.SendMessage(
                                $"**Exception**\n{exception.InnerException.Message}\n```\n{exception.InnerException}```");
                        }
                        break;
                    case CommandError.BadArgCount:
                        await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], result.ErrorReason));
                        break;
                    case CommandError.UnknownCommand:
                        break;
                    case CommandError.ParseFailed:
                        await context.Channel.SendMessageAsync($"", embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"Couldn't parse entered value! Make sure you enter the requested data type").WithDescription("If a whole number is asked then please provide one etc."));
                        break;
                    default:
                        await context.Channel.SendMessageAsync($"", embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"{result.ErrorReason}"));
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