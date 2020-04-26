using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Options;
using SoraBot.Common.Extensions.Modules;
using SoraBot.Data.Configurations;

namespace SoraBot.Bot.Modules
{
    [Name("Misc")]
    [Summary("Miscellaneous commands")]
    public class MiscModule : SoraSocketCommandModule
    {
        private readonly SoraBotConfig _config;

        public MiscModule(IOptions<SoraBotConfig> conf)
        {
            _config = conf?.Value ?? throw new ArgumentNullException(nameof(conf));
        }

        [Command("inivite"), Alias("inv")]
        [Summary("Gives you the invite link to invite Sora")]
        public async Task InviteSora()
            => await ReplyEmbed(new EmbedBuilder()
            {
                Color = Purple,
                Title = "✉️ Invite Sora to your Guild (click me)",
                Description = "The selected permissions are the needed permissions if you intend to use " +
                              "ALL of Sora's features. If you dont intend to use the moderation or co you dont " +
                              "need to give him all the perms. He just wont be able to do certain things if he misses " +
                              "some permissions.",
                Url = _config.SoraBotInvite,
                ThumbnailUrl = Context.Client.CurrentUser.GetAvatarUrl(),
                Footer = RequestedByMe()
            });
    }
}