using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using SoraBot_v2.Services;

namespace SoraBot_v2.Module
{
    public class ProfileModule : ModuleBase<SocketCommandContext>
    {
        private ProfileService _profileService;

        public ProfileModule(ProfileService service)
        {
            _profileService = service;
        }
        
        [Command("removebackground"), Alias("removebg", "rbg", "defaultcard"),
         Summary("Removes your current BG and resets you to the default profile card")]
        public async Task RemoveBackGround()
        {
            await _profileService.RemoveBg(Context);
        }
        
        [Command("profile", RunMode = RunMode.Async), Alias("p"), Summary("Shows your profile Card")]
        public async Task ProfileCard(SocketUser userT = null)
        {
            var user = userT ?? Context.User;
            if (user.IsBot)
            {
                await ReplyAsync("",
                    embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Bots don't have a profile!"));
                return;
            }
            var typing = Context.Channel.EnterTypingState();
            typing.Dispose();
            await _profileService.DrawProfileCard(Context, user);
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
            if (!url.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) && !url.EndsWith(".png", StringComparison.OrdinalIgnoreCase) && !url.EndsWith(".gif", StringComparison.OrdinalIgnoreCase) && !url.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
            {
                await Context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "You must link or attach an Image!"));
                return;
            }
            await _profileService.SetCustomBg(url, Context);
        }
    }
}