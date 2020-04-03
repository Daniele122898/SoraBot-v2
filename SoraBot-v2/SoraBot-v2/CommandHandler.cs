using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using SoraBot_v2.Data;
using SoraBot_v2.Services;

namespace SoraBot_v2
{
    public class CommandHandler
    {
        public static ulong MessagesReceived;
        public static int CommandsExecuted;

        private IServiceProvider _services;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly AfkService _afkService;
        private readonly RatelimitingService _ratelimitingService;
        private readonly GuildCountUpdaterService _guildCount;
        private readonly BanService _banService;
        private readonly InteractionsService _interactionsService;
        // private readonly LavaSocketClient _lavaSocketClient;
        // private readonly AudioService _audioService;

        private Task ClientOnJoinedGuild(SocketGuild socketGuild)
        {
            Task.Run(async () =>
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
                    await soraContext.SaveChangesAsync();
                    try
                    {
                        string prefix = Utility.GetGuildPrefix(socketGuild, soraContext);
                        await (await socketGuild.Owner.GetOrCreateDMChannelAsync()).SendMessageAsync("",
                            embed: Utility.ResultFeedback(Utility.BlueInfoEmbed, Utility.SuccessLevelEmoji[3],
                                    $"Hello there (≧∇≦)/")
                                .WithDescription($"I'm glad you invited me over :)\n" +
                                                 $"You can find the list of commands and help by using `{prefix}help`.\n" +
                                                 $"To restrict tag creation and Sora's mod functions you must create\n" +
                                                 $"a {Utility.SORA_ADMIN_ROLE_NAME} Role so that only the ones carrying it can create\n" +
                                                 $"tags or use Sora's mod functionality. You can make him create one with: " +
                                                 $"`{prefix}createAdmin`\n" +
                                                 $"You can leave tag creation unrestricted if you want but its not\n" +
                                                 $"recommended on larger servers as it will be spammed.\n" +
                                                 $"PS: Standard Prefix is `$` but you can change it with:\n" +
                                                 $"`@Sora prefix yourPrefix`\n")
                                .WithThumbnailUrl(socketGuild.IconUrl ?? Utility.StandardDiscordAvatar)
                                .AddField("Support", $"You can find the [support guild here]({Utility.DISCORD_INVITE})")
                                .Build());
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }

                //inform me of joining
                await SentryService.SendMessage(
                    $"**JOINED GUILD**\nName: {socketGuild.Name}\nID: {socketGuild.Id}\nUsers: {socketGuild.MemberCount}\nOwner: {Utility.GiveUsernameDiscrimComb(socketGuild.Owner)}");
                //TODO WELCOME MESSAGE
            });

            return Task.CompletedTask;
        }

        public CommandHandler(IServiceProvider provider, DiscordSocketClient client, CommandService commandService,
            AfkService afkService, RatelimitingService ratelimitingService, StarboardService starboardService,
            SelfAssignableRolesService selfService, AnnouncementService announcementService,
            ModService modService, GuildCountUpdaterService guildUpdate, ExpService expService,
            BanService banService, InteractionsService interactionsService)
        {
            _client = client;
            _commands = commandService;
            _afkService = afkService;
            _services = provider;
            _ratelimitingService = ratelimitingService;
            _guildCount = guildUpdate;
            _banService = banService;
            _interactionsService = interactionsService;
            // _lavaSocketClient = lavaSocketClient;
            // _audioService = audioService;

            _guildCount.Initialize(client.ShardId, Utility.TOTAL_SHARDS, client.Guilds.Count);


            _client.MessageReceived += HandleCommandsAsync;
            //_client.MessageReceived += _afkService.Client_MessageReceived;
            _commands.Log += CommandsOnLog;
            _client.JoinedGuild += ClientOnJoinedGuild;
            _client.LeftGuild += ClientOnLeftGuild;
            _client.MessageReceived += expService.IncreaseEpOnMessageReceive;
            _client.ReactionAdded += starboardService.ClientOnReactionAdded;
            _client.ReactionRemoved += starboardService.ClientOnReactionRemoved;
            _client.UserJoined += selfService.ClientOnUserJoined;
            _client.UserJoined += announcementService.ClientOnUserJoined;
            _client.UserLeft += announcementService.ClientOnUserLeft;

            //mod Service
            _client.UserBanned += modService.ClientOnUserBanned;
            _client.UserUnbanned += modService.ClientOnUserUnbanned;

            // Ready
            _client.Ready += ClientOnReady;
        }

        // private Task LavaSocketClientOnLog(LogMessage msg)
        // {
        //     switch (msg.Severity)
        //     {
        //         case LogSeverity.Critical:
        //         case LogSeverity.Error:
        //             Console.ForegroundColor = ConsoleColor.Red;
        //             break;
        //         case LogSeverity.Warning:
        //             Console.ForegroundColor = ConsoleColor.Yellow;
        //             break;
        //         case LogSeverity.Info:
        //             Console.ForegroundColor = ConsoleColor.White;
        //             break;
        //         case LogSeverity.Verbose:
        //         case LogSeverity.Debug:
        //             Console.ForegroundColor = ConsoleColor.DarkGray;
        //             break;
        //     }
        //
        //     Console.WriteLine($"{DateTime.Now,-19} [{msg.Severity,8}] {msg.Source}: {msg.Message} {msg.Exception}");
        //     Console.ResetColor();
        //     return Task.CompletedTask;
        // }

        private Task ClientOnReady()
        {
            SentryService.Install(_client);
            /*
            // lavalink shit
            _lavaSocketClient.Log += LavaSocketClientOnLog;
            // setup lavalink
            var conf = new Configuration()
            {
                Host = ConfigService.GetConfigData("lavalinkip"),
                Port = ushort.Parse(ConfigService.GetConfigData("lavalinkport")),
                BufferSize = 1024,
                ReconnectAttempts = 3,
                Password = ConfigService.GetConfigData("lavalinkpw"),
                LogSeverity = LogSeverity.Info
            };

            // We want to NOT await this as it blocks the gw thread. this should run on a separate thread.
            Task.Run(async () =>
            {
                await _lavaSocketClient.StartAsync(_client, conf);

                LavaRestClient lavaRestClient = new LavaRestClient(conf);

                _audioService.Initialize(_lavaSocketClient, lavaRestClient, _client.CurrentUser.Id);
                // voice shit
                _client.UserVoiceStateUpdated += _audioService.ClientOnUserVoiceStateUpdated;
                // _client.Disconnected += _audioService.ClientOnDisconnected;
            });
            */

            return Task.CompletedTask;
        }

        private Task ClientOnLeftGuild(SocketGuild socketGuild)
        {
            // For mass leaving
            Task.Run(async () =>
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

                await SentryService.SendMessage(
                    $"**LEFT GUILD**\nName: {socketGuild.Name}\nID: {socketGuild.Id}\nUsers: {socketGuild.MemberCount}\nOwner: {Utility.GiveUsernameDiscrimComb(socketGuild.Owner)}");
            });
            return Task.CompletedTask;
        }

        public async Task InitializeAsync(IServiceProvider provider)
        {
            _services = provider;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
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

                // Check if invoker is banned
                if (_banService.IsBanned(message.Author.Id))
                    return;

                // Check the permissions of this channel 
                if (await Utility.CheckReadWritePerms(channel.Guild, channel, false).ConfigureAwait(false) == false)
                    return;

                // Check the ratelimit of this author
                if (await _ratelimitingService.IsRatelimited(message.Author.Id).ConfigureAwait(false))
                    return;

                // Permissions are present and author is eligible for commands.
                //Hand it over to the AFK Service to do its thing. Don't await to not block command processing. 
                var _ = _afkService.Client_MessageReceived(m, _services).ConfigureAwait(false);
                // Look for a prefix but use a hardcoded fallback instead of creating a default guild.
                string prefixCacheId = CacheService.DISCORD_GUILD_PREFIX + channel.Guild.Id.ToString();
                var prefix = await CacheService.GetOrSetAsync<string>(prefixCacheId, async () =>
                {
                    // Get a database instance. 
                    using var soraContext = new SoraContext();
                    var guild = await soraContext.Guilds.FindAsync(channel.Guild.Id);
                    if (guild == null) return "$";

                    return guild.Prefix;
                }).ConfigureAwait(false);

                // Check if the message starts with the prefix or mention before doing anything else.
                // Also rely on stdlib stuff for that because #performance.

                int argPos = prefix.Length - 1;
                if (!(message.HasStringPrefix(prefix, ref argPos) ||
                      message.HasMentionPrefix(_client.CurrentUser, ref argPos)))
                    return;

                // Detection finished.
                // We know it's *very likely* a command for us.
                // It's safe to create a context now.
                var context = new SocketCommandContext(_client, message);

                // Also allocate a default guild if needed since we skipped that part earlier.
                // await Utility.CreateGuildIfNeeded(channel.Guild.Id, soraContext);
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
                    await HandleErrorAsync(result, context).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private async Task HandleErrorAsync(IResult result, SocketCommandContext context,
            CommandException exception = null)
        {
            switch (result.Error)
            {
                case CommandError.Exception:
                    if (exception != null && exception.InnerException != null)
                    {
                        await SentryService.SendMessage(
                            $"**Exception**\n{exception.InnerException.Message}\n```\n{exception.InnerException}```");
                    }

                    break;
                case CommandError.BadArgCount:
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            result.ErrorReason).Build());
                    break;
                case CommandError.UnknownCommand:
                    break;
                case CommandError.ParseFailed:
                    await context.Channel.SendMessageAsync($"",
                        embed: Utility.ResultFeedback(
                                Utility.RedFailiureEmbed,
                                Utility.SuccessLevelEmoji[2],
                                $"Couldn't parse entered value! Make sure you enter the requested data type")
                            .WithDescription("If a whole number is asked then please provide one. " +
                                             "If two strings are asked or smth after the first string please " +
                                             "wrap the string in \"\" if it consists of more than one word!\n" +
                                             "If you are using a @mention it sometimes fails if the user has not been seen for a while. " +
                                             "If that is the case use his User Id instead of the mention.")
                            .Build());
                    break;
                default:
                    await context.Channel.SendMessageAsync($"",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            $"{result.ErrorReason}").Build());
                    break;
            }
        }

        private async Task HandleErrorAsync(LogMessage logMessage)
        {
            var commandException = logMessage.Exception as CommandException;
            if (commandException == null) return;
            await HandleErrorAsync(ExecuteResult.FromError(commandException),
                (SocketCommandContext) commandException.Context, commandException);
        }
    }
}