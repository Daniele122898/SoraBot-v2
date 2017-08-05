using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using SoraBot_v2.Data;
using SoraBot_v2.Services;

namespace SoraBot_v2.Module
{
    public class DynamicPrefixModule: ModuleBase<SocketCommandContext>
    {
        private DynamicPrefixService _prefixService;
        private SoraContext _soraContext;

        public DynamicPrefixModule(DynamicPrefixService dynamicPrefixService, SoraContext soraContext)
        {
            _prefixService = dynamicPrefixService;
            _soraContext = soraContext;
        }

        [Command("prefix"), Summary("Changes the prefix of the bot for this guild")]
        public async Task ChangeGuildPrefix([Summary("Prefix to change to")] string prefix)
        {
            if (!((SocketGuildUser) Context.User).GuildPermissions.Has(GuildPermission.Administrator))
            {
                await ReplyAsync("",
                    embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                        "You don't have permission to set the prefix! You need Administrator permissions!"));
                return;
            }
            await _prefixService.UpdateGuildPrefix(Context, _soraContext, prefix);
        }

        [Command("prefix"), Summary("Shows the current prefix for the Guild")]
        public async Task CheckPrefix()
        {
            await _prefixService.ReturnGuildPrefix(Context, _soraContext);
        }
    }
}