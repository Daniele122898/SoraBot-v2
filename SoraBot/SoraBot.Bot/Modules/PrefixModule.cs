using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using SoraBot.Common.Extensions.Modules;
using SoraBot.Services.Guilds;

namespace SoraBot.Bot.Modules
{
    [Name("Prefix")]
    [Summary("Commands to get and set the prefix inside a Guild")]
    public class PrefixModule : SoraSocketCommandModule
    {
        private readonly IPrefixService _prefixService;

        public PrefixModule(IPrefixService prefixService)
        {
            _prefixService = prefixService;
        }

        [Command("prefix")]
        [Summary("Gets the current prefix in the guild")]
        public async Task GetPrefix()
        {
            var prefix = await _prefixService.GetPrefix(Context.Guild.Id).ConfigureAwait(false);
            await ReplySuccessEmbed($"The Prefix for this Guild is `{prefix}`");
        }

        [Command("prefix")]
        [Summary("This lets you change the prefix in the Guild. " +
                 "You need to be an Administrator to do this!")]
        [RequireUserPermission(GuildPermission.Administrator, ErrorMessage = "You must be an Administrator to use this command!")]
        public async Task SetPrefix(string prefix)
        {
            prefix = prefix.Trim();
            if (prefix.Length > 20)
            {
                await ReplyFailureEmbed("Please specify a prefix that is shorter than 20 Characters!");
                return;
            }

            if (!await _prefixService.SetPrefix(Context.Guild.Id, prefix).ConfigureAwait(false))
            {
                await ReplyFailureEmbed("Something failed when trying to save the prefix. Please try again");
                return;
            }

            await ReplySuccessEmbed($"Successfully updated Guild Prefix to `{prefix}`!");
        }
    }
}