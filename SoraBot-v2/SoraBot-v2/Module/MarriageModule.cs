using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using SoraBot_v2.Services;

namespace SoraBot_v2.Module
{
    [Name("Marriage")]
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
                        "I'm sorry. You can't marry bots :/").Build());
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
        public async Task Divorce(ulong id)
        {
            await _marriageService.Divorce(Context, id);
        }

        [Command("marriages"), Alias("marrylist"), Summary("Shows all your marriages")]
        public async Task ShowMarriages(SocketUser userT = null, string tags = null)
        {
            var user = userT ?? Context.User;
            bool adv = false;
            if (!string.IsNullOrWhiteSpace(tags))
            {
                if (tags.Equals("-adv", StringComparison.OrdinalIgnoreCase))
                {
                    adv = true;
                }
            }
            await _marriageService.ShowMarriages(Context, user, adv);
        }

        [Command("marrylimit"), Alias("checklimit", "marriagelimit"), Summary("Checks your marriage limit")]
        public async Task MarriageLimit(SocketUser userT = null)
        {
            var user = userT ?? Context.User;
            await _marriageService.CheckLimit(Context, user);
        }
    }
}