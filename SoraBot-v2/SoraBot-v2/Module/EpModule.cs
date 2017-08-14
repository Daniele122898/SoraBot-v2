using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using SoraBot_v2.Services;

namespace SoraBot_v2.Module
{
    public class EpModule: ModuleBase<SocketCommandContext>
    {
        private EpService _epService;

        public EpModule(EpService epService)
        {
            _epService = epService;
        }
        
        [Command("togglenotify"), Alias("subscribe", "sub", "subep"), Summary("Will notify you when you level up!")]
        public async Task ToggleNotify()
        {
            await _epService.ToggleEpGain(Context);
        }

        [Command("profile", RunMode = RunMode.Async), Alias("p"), Summary("Shows your profile Card")]
        public async Task ProfileCard(SocketUser userT = null)
        {
            var typing = Context.Channel.EnterTypingState();
            var user = userT ?? Context.User;
            await _epService.DrawProfileCard(Context, user);
            typing.Dispose();
        }

        [Command("removebackground"), Alias("removebg", "rbg", "defaultcard"),
         Summary("Removes your current BG and resets you to the default profile card")]
        public async Task RemoveBackGround()
        {
            await _epService.RemoveBg(Context);
        }

        [Command("top10"), Alias("top", "localtop", "localtop10", "t10"),
         Summary("Shows the top 10 users in this guild by EXP")]
        public async Task LocalTop10()
        {
            await _epService.GetLocalTop10List(Context);
        }
        
        [Command("globaltop10"), Alias("globaltop", "globalt", "top10globally", "gt10", "global top10", "gtop10"),
         Summary("Shows the top 10 users globally")]
        public async Task GlobalTop10()
        {
            await _epService.GetGlobalTop10(Context);
        }

        [Command("setbackground", RunMode = RunMode.Async), Alias("setbg", "sbg"),
         Summary("Give Sora any Link to any Picture and he will use it as your profile card Background!")]
        public async Task SetBg(string url = null)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                if (Context.Message.Attachments.Count < 1)
                {
                    await ReplyAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "If you do not specify a link to an Image then please attach one!"));
                    return;
                }
                else if (Context.Message.Attachments.Count > 1)
                {
                    await ReplyAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Please only attach one Image!"));
                    return;                   
                }
                url = Context.Message.Attachments.ToArray()[0].Url;
            }
            if (!url.EndsWith(".jpg") && !url.EndsWith(".png") && !url.EndsWith(".gif") && !url.EndsWith(".jpeg"))
            {
                await Context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "You must link or attach an Image!"));
                return;
            }
            await _epService.SetCustomBg(url, Context);
        }
    }
}