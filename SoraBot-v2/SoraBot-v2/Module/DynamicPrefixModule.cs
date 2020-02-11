using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using SoraBot_v2.Data;
using SoraBot_v2.Services;

namespace SoraBot_v2.Module
{
    [Name("Misc")]
    public class DynamicPrefixModule : ModuleBase<SocketCommandContext>
    {
        private DynamicPrefixService _prefixService;

        public DynamicPrefixModule(DynamicPrefixService dynamicPrefixService)
        {
            _prefixService = dynamicPrefixService;
        }

        [Command("prefix"), Summary("Changes the prefix of the bot for this guild")]
        public async Task ChangeGuildPrefix([Summary("Prefix to change to")] string prefix)
        {
            var user = ((SocketGuildUser)Context.User);
            if (!user.GuildPermissions.Has(GuildPermission.Administrator) && !Utility.IsSoraAdmin(user))
            {
                await ReplyAsync("",
                    embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                        $"You don't have permission to set the prefix! You need Administrator permissions or the {Utility.SORA_ADMIN_ROLE_NAME} role!").Build());
                return;
            }
            if (string.IsNullOrWhiteSpace(prefix))
            {
                await ReplyAsync("",
                    embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                        "Prefix can't be null or whitespace!").Build());
                return;
            }

            using (var soraContext = new SoraContext())
            {
                await _prefixService.UpdateGuildPrefix(Context, soraContext, prefix);
            }
        }

        [Command("prefix"), Summary("Shows the current prefix for the Guild")]
        public async Task CheckPrefix()
        {
            using (var soraContext = new SoraContext())
            {
                await _prefixService.ReturnGuildPrefix(Context, soraContext);
            }
        }
    }
}