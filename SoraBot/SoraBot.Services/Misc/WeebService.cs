using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Logging;
using SoraBot.Common.Extensions.Modules;
using SoraBot.Common.Utils;
using Weeb.net;
using TokenType = Weeb.net.TokenType;

namespace SoraBot.Services.Misc
{
    public class WeebService
    {
        public WeebClient WeebClient { get; private set; }
        public bool IsAuthenticated { get; private set; }

        private readonly ILogger<WeebService> _log;

        public WeebService(ILogger<WeebService> log)
        {
            _log = log;
        }
        
        public async Task<bool> TryAuthenticate(string token)
        {
            WeebClient = new WeebClient("Sora", "3.0.0");
            try
            {
                await WeebClient.Authenticate(token, TokenType.Bearer);
                this.IsAuthenticated = true;
                return true;
            }
            catch (Exception e)
            {
                _log.LogError(e, "Couldn't authenticate WeebClient");
                this.IsAuthenticated = false;
                return false;
            }
        }

        public async Task AddInteractions(CommandService service)
        {
            if (!this.IsAuthenticated) return;
            var types = await WeebClient.GetTypesAsync().ConfigureAwait(false);
            if (types == null || types.Types.Count == 0) return;
            await service.CreateModuleAsync("", build =>
            {
                build.Name = "Interactions";
                build.Summary = "All available interaction commands";

                foreach (var type in types.Types)
                {
                    build.AddCommand(type, async (context, objects, serviceProvider, commandInfo) =>
                    {
                        var image = await this.WeebClient.GetRandomAsync(type, Array.Empty<string>(), FileType.Any,
                                false, NsfwSearch.False).ConfigureAwait(false);
                        if (image == null)
                        {
                            await context.Channel.SendMessageAsync("", embed: new EmbedBuilder()
                            {
                                Color = SoraSocketCommandModule.Red,
                                Title = $"{SoraSocketCommandModule.FailureEmoji} Failed to fetch image :/ Try another one."
                            }.Build());
                            return;
                        }
                        
                        var eb = new EmbedBuilder()
                        {
                            Color = SoraSocketCommandModule.Purple,
                            Footer = new EmbedFooterBuilder()
                            {
                                Text = "Powered by weeb.sh and the weeb.net wrapper"
                            },
                            ImageUrl = image.Url
                        };

                        var mentions = context.Message.MentionedUserIds
                            .Where(id => id != context.User.Id && id != context.Client.CurrentUser.Id)
                            .ToArray();
                        if (mentions.Length == 0)
                        {
                            await context.Channel.SendMessageAsync("", embed: eb.Build());
                            return;
                        }
                        // Otherwise create a nice title
                        var tasks = mentions
                            .Select(async x => await context.Guild.GetUserAsync(x).ConfigureAwait(false));
                        var res = (await Task.WhenAll(tasks).ConfigureAwait(false))
                            .Where(u => u != null)
                            .Select(Formatter.UsernameDiscrim);
                        var title = String.Join(", ", res);
                        if (title.Length > 150)
                            title = $"{title.Remove(150)}...";

                        title = GetTitle(type, Formatter.UsernameDiscrim(context.User), title);
                        if (title != null)
                        {
                            eb.Title = title;
                        }
                        await context.Channel.SendMessageAsync("", embed: eb.Build());
                    }, builder =>
                    {
                        builder.AddParameter("users", typeof(string),
                            parameterBuilder =>
                            {
                                parameterBuilder.IsRemainder = true;
                                parameterBuilder.IsOptional = true;
                            });
                    });
                }
            });
        }

        private static string GetTitle(string type, string userName, string mentioned)
        {
            switch (type)
            {
                case "cuddle":
                    return $"{userName} cuddled {mentioned} °˖✧◝(⁰▿⁰)◜✧˖°";
                case "insult":
                    return $"{userName} insulted {mentioned} (⌯˃̶᷄ ﹏ ˂̶᷄⌯)ﾟ";
                case "lick":
                    return $"{userName} licked {mentioned} ༼ つ ◕o◕ ༽つ";
                case "nom":
                    return $"{userName} nommed {mentioned} (*ﾟﾛﾟ)";
                case "stare":
                    return $"{userName} stares at {mentioned} ⁄(⁄ ⁄•⁄-⁄•⁄ ⁄)⁄";
                case "tickle":
                    return $"{userName} tickled {mentioned} o(*≧□≦)o";
                case "bite":   
                    return $"{userName} bit {mentioned} (˃̶᷄︿๏）";
                case "greet":
                    return $"{userName} greeted {mentioned} (=ﾟωﾟ)ノ";
                case "hug":
                    return $"{userName} hugged {mentioned} °˖✧◝(⁰▿⁰)◜✧˖°";
                case "kiss":
                    return $"{userName} kissed {mentioned} (✿ ♥‿♥)♥";
                case "pat":
                    return $"{userName} patted {mentioned} ｡◕ ‿ ◕｡";
                case "poke":
                    return $"{userName} poked {mentioned} ( ≧Д≦)";
                case "slap":
                    return $"{userName} slapped {mentioned} (ᗒᗩᗕ)՞ ";
                case "highfive":
                    return $"{userName} highfived {mentioned} °˖✧◝(⁰▿⁰)◜✧˖°";
                case "punch":
                    return $"{userName} punched {mentioned} (ᗒᗩᗕ)՞ ";
                default:
                    return null;
            }
        }
    }
}