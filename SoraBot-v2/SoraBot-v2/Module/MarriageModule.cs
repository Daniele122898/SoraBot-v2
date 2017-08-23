using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using SoraBot_v2.Services;

namespace SoraBot_v2.Module
{
    public class MarriageModule : ModuleBase<SocketCommandContext>
    {
        private MarriageService _marriageService;

        public MarriageModule(MarriageService marriageService)
        {
            _marriageService = marriageService;
        }

        [Command("marry", RunMode = RunMode.Async), Summary("Marries specified person if agreed")]
        public async Task Marry(SocketUser user)
        {
            if (user.IsBot)
            {
                await Context.Channel.SendMessageAsync("", embed:
                    Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                        "I'm sorry. You can't marry bots :/"));
                return;
            }
            await _marriageService.Marry(Context, user);
        }

        [Command("divorce")]
        public async Task Divorce(SocketUser user)
        {
            await _marriageService.Divorce(Context, user.Id);
        }
        
        [Command("divorce")]
        public async Task Divorce(ulong Id)
        {
            await _marriageService.Divorce(Context, Id);
        }

        [Command("marriages"), Alias("marrylist"), Summary("Shows all your marriages")]
        public async Task ShowMarriages(SocketUser userT = null)
        {
            var user = userT ?? Context.User;
            await _marriageService.ShowMarriages(Context, user);
        }

        [Command("marrylimit"), Alias("checklimit", "marriagelimit"), Summary("Checks your marriage limit")]
        public async Task MarriageLimit(SocketUser userT = null)
        {
            var user = userT ?? Context.User;
            await _marriageService.CheckLimit(Context, user);
        }
    }
}