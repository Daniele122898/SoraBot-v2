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
        public static double MessagesReceived;
        public static int CommandsExecuted;

        private IServiceProvider _services;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly AfkService _afkService;
        private StarboardService _starboardService;
        private readonly RatelimitingService _ratelimitingService;
        private SelfAssignableRolesService _selfAssignableRolesService;
        private AnnouncementService _announcementService;
        private ModService _modService;
        private readonly GuildCountUpdaterService _guildCount;
        private BanService _banService;
        private InteractionsService _interactionsService;

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
            using (var soraContext = new SoraContext())
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
                                         $"You can find the [list of commands and help here](http://git.argus.moe/serenity/SoraBot-v2/wikis/home)\n" +
                                         $"To restrict tag creation and Sora's mod functions you must create\n" +
                                         $"a {Utility.SORA_ADMIN_ROLE_NAME} Role so that only the ones carrying it can create\n" +
                                         $"tags or use Sora's mod functionality. You can make him create one with: " +
                                         $"`{prefix}createAdmin`\n" +
                                         $"You can leave tag creation unrestricted if you want but its not\n" +
                                         $"recommended on larger servers as it will be spammed.\n" +
                                         $"**Sora now has a Dashboard**\n" +
                                         $"You can [find the dashboard here](http://argonaut.pw/Sora/) by clicking the login\n"+
                                         $"button in the top right. It's still in pre-alpha but allows you to\n"+
                                         $"customize levels and level rewards as well as other settings. It is required\n" + 
                                         $"for proper setup of leveling.\n"+
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

        public CommandHandler(IServiceProvider provider, DiscordSocketClient client, CommandService commandService,
            AfkService afkService, RatelimitingService ratelimitingService, StarboardService starboardService, SelfAssignableRolesService selfService, AnnouncementService announcementService,
            ModService modService, GuildCountUpdaterService guildUpdate, ExpService expService, BanService banService, InteractionsService interactionsService)
        {
            _client = client;
            _commands = commandService;
            _afkService = afkService;
            _services = provider;
            _ratelimitingService = ratelimitingService;
            _starboardService = starboardService;
            _selfAssignableRolesService = selfService;
            _announcementService = announcementService;
            _modService = modService;
            _guildCount = guildUpdate;
            _banService = banService;
            _interactionsService = interactionsService;
            
            _guildCount.Initialize(client.ShardId, Utility.TOTAL_SHARDS, client.Guilds.Count);


            _client.MessageReceived += HandleCommandsAsync;
            //_client.MessageReceived += _afkService.Client_MessageReceived;
            _commands.Log += CommandsOnLog;
            _client.JoinedGuild += ClientOnJoinedGuild;
            _client.LeftGuild += ClientOnLeftGuild;
            _client.MessageReceived += expService.IncreaseEpOnMessageReceive;
            _client.ReactionAdded += _starboardService.ClientOnReactionAdded;
            _client.ReactionRemoved += _starboardService.ClientOnReactionRemoved;
            _client.UserJoined += _selfAssignableRolesService.ClientOnUserJoined;
            _client.UserJoined += _announcementService.ClientOnUserJoined;
            _client.UserLeft += _announcementService.ClientOnUserLeft;

            //mod Service
            _client.UserBanned += _modService.ClientOnUserBanned;
            _client.UserUnbanned += _modService.ClientOnUserUnbanned;
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
            // create interactions
            await _interactionsService.AddOtherCommands(_commands);
        }

        private Task CommandsOnLog(LogMessage logMessage)
        {
            return HandleErrorAsync(logMessage);
        }

        private async Task HandleCommandsAsync(SocketMessage m)
        {
            try
            {
                // Stats and stuff.
                MessagesReceived++;

                // Make sure that this object can be a SocketUserMessage before processing anything.
                // Then store the cast in "message" with C# magic.
                // Exit otherwise.
                if (!(m is SocketUserMessage message)) return;

                // Only continue if the author is not a bot.
                if (message.Author.IsBot) return;

                // Try to extract and upcast the channel.
                // Same logic as before but without SocketContext overhead.
                if (!(m.Channel is SocketGuildChannel channel))
                {
                    // TODO: Should we mock the D.NET behaviour here?
                    // They seem to assign null if the cast is not possible.
                    // This needs more investigation.

                    // For now we'll just exit here.
                    return;
                }
                
                // Check if invoker is banend
                if (_banService.IsBanned(message.Author.Id))
                    return;

                // Check the permissions of this channel 
                if (await Utility.CheckReadWritePerms(channel.Guild, channel, false) == false)
                    return;

                // Check the ratelimit of this author
                if (await _ratelimitingService.IsRatelimited(message.Author.Id))
                    return;

                // Permissions are present and author is eligible for commands.
                // Get a database instance. 
                using (var soraContext = new SoraContext())
                {
                    //Hand it over to the AFK Service to do its thing. Don't await to not block command processing. 
                    _afkService.Client_MessageReceived(m, _services);

                    // Look for a prefix but use a hardcoded fallback instead of creating a default guild.
                    // TODO: Move this into the config file
                    var prefix = Utility.GetGuildPrefixFast(soraContext, channel.Guild.Id, "$");

                    // Check if the message starts with the prefix or mention before doing anything else.
                    // Also rely on stdlib stuff for that because #performance.

                    int argPos = prefix.Length - 1;
                    if (!(message.HasStringPrefix(prefix, ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos)))
                        return;

                    // Detection finished.
                    // We know it's *very likely* a command for us.
                    // It's safe to create a context now.
                    var context = new SocketCommandContext(_client, message);

                    // Also allocate a default guild if needed since we skipped that part earlier.
                    Utility.GetOrCreateGuild(channel.Guild.Id, soraContext);
                    // Handoff control to D.NET
                    var result = await _commands.ExecuteAsync(
                        context,
                        argPos,
                        _services
                    );

                    // Handle errors if needed          
                    if (result.IsSuccess)
                    {
                        CommandsExecuted++;
                        _ratelimitingService.RateLimitMain(context.User.Id);
                    }
                    else
                    {
                        //await context.Channel.SendMessageAsync($"**FAILED**\n{result.ErrorReason}");
                        await HandleErrorAsync(result, context);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


        /*private async Task HandleCommandsAsync(SocketMessage m)
        {
            try
            {
                MessagesReceived++;
                var message = m as SocketUserMessage;
                if (message == null) return;
                if (message.Author.IsBot) return;
                if (!(message.Channel is SocketGuildChannel)) return;
                
                //create Context
                var context = new SocketCommandContext(_client,message);
                //Check essential perms, set send message to false here to prevent spam from normal failiure. 
                //darwinism :P
                if(await Utility.CheckReadWritePerms(context.Guild, (IGuildChannel)context.Channel, false) == false)
                    return;
                
                //Hand to AFK service
                
                Task.Run(async () =>
                {
                    using (var soraContext = new SoraContext())
                    {
                        await _afkService.Client_MessageReceived(m, soraContext);
                    }
                });*
                
            
                //prefix ends and command starts
                string prefix = Utility.GetGuildPrefix(context.Guild, _soraContext);
                
               
                //prefix = Utility.GetGuildPrefix(context.Guild, soraContext);
                
                int argPos = prefix.Length-1;
                if(!(message.HasStringPrefix(prefix, ref argPos)|| message.HasMentionPrefix(_client.CurrentUser, ref argPos)))
                    return;
            
                //Check ratelimit
                if(await _ratelimitingService.IsRatelimited(message.Author.Id))
                    return;
                
                var result = await _commands.ExecuteAsync(context, argPos, _services);
                //LOG
                //Logger.WriteRamLog(context);
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
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }*/

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
                    await context.Channel.SendMessageAsync($"", embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"Couldn't parse entered value! Make sure you enter the requested data type").WithDescription("If a whole number is asked then please provide one. If two strings are asked or smth after the first string please wrap the string in \"\" if it consists of more than one word!"));
                    break;
                default:
                    await context.Channel.SendMessageAsync($"", embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"{result.ErrorReason}"));
                    break;
            }
        }

        private async Task HandleErrorAsync(LogMessage logMessage)
        {
            var commandException = logMessage.Exception as CommandException;
            if (commandException == null) return;
            await HandleErrorAsync(ExecuteResult.FromError(commandException),
                (SocketCommandContext)commandException.Context, commandException);
        }
    }
}