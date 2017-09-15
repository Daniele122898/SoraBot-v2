using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using SoraBot_v2.Services;

namespace SoraBot_v2.Module
{
    public class AnnouncementModule : ModuleBase<SocketCommandContext>
    {
        private AnnouncementService _announcement;

        public AnnouncementModule(AnnouncementService service)
        {
            _announcement = service;
        }

        [Command("welcomemsg"), Alias("welcomemessage", "wmsg", "setwelcomemsg"),
         Summary("Sets the User welcome message!")]
        public async Task SetWelcomeMsg([Remainder] string message)
        {
            if (await _announcement.SetWelcomeMessage(Context, message))
            {
                await ReplyAsync("", embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed,
                    Utility.SuccessLevelEmoji[0], "Successfully set Welcome message to").WithDescription(message));
            }
        }

        [Command("leavemsg"), Alias("leavemessage", "lmsg", "setleavemsg"), Summary("Sets the user leave message!")]
        public async Task SetLeaveMsg([Remainder] string message)
        {
            if (await _announcement.SetLeaveMessage(Context, message))
            {
                await ReplyAsync("", embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed,
                    Utility.SuccessLevelEmoji[0], "Successfully set Leave message to").WithDescription(message));
            }
        }

        [Command("welcomechan"), Alias("welcomechannel", "wchan", "setwelcomechan"),
         Summary("Sets the Welcome announcement channel")]
        public async Task SetWelcomeChan(SocketChannel channel)
        {
            if (await _announcement.SetWelcomeChannel(Context, channel))
            {
                await ReplyAsync("", embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed,
                    Utility.SuccessLevelEmoji[0], "Successfully set Welcome channel to").WithDescription($"<#{channel.Id}>"));
            }
        }
        
        [Command("leavechan"), Alias("leavechannel", "lchan", "setleavechan"), Summary("Sets the leave announcement channel")]
        public async Task SetLeaveChan(SocketChannel channel)
        {
            if (await _announcement.SetLeaveChannel(Context, channel))
            {
                await ReplyAsync("", embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed,
                    Utility.SuccessLevelEmoji[0], "Successfully set Leave channel to").WithDescription($"<#{channel.Id}>"));
            }
        }

        [Command("welcome"), Alias("setwelcome"), Summary("Sets the welcome message and channel")]
        public async Task SetWelcome(SocketChannel channel, [Remainder] string message)
        {
            if (await _announcement.SetWelcomeChannel(Context, channel))
            {
                if (await _announcement.SetWelcomeMessage(Context, message))
                {
                    await ReplyAsync("", embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed,
                        Utility.SuccessLevelEmoji[0], "Successfully set Welcome to").WithDescription($"{message} and with channel: <#{channel.Id}>"));
                }
                else
                {
                    await ReplyAsync("", embed: Utility.ResultFeedback(Utility.YellowWarningEmbed,
                        Utility.SuccessLevelEmoji[1], "Only set Welcome channel to").WithDescription($"<#{channel.Id}>\nFailed to add message"));
                }
            }
        }
        
        [Command("leave"), Alias("setleave"), Summary("Sets the leave message and channel")]
        public async Task SetLeave(SocketChannel channel, [Remainder] string message)
        {
            if (await _announcement.SetLeaveChannel(Context, channel))
            {
                if (await _announcement.SetLeaveMessage(Context, message))
                {
                    await ReplyAsync("", embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed,
                        Utility.SuccessLevelEmoji[0], "Successfully set Leave to").WithDescription($"{message} and with channel: <#{channel.Id}>"));
                }
                else
                {
                    await ReplyAsync("", embed: Utility.ResultFeedback(Utility.YellowWarningEmbed,
                        Utility.SuccessLevelEmoji[1], "Only set Leave channel to").WithDescription($"<#{channel.Id}>\nFailed to add message"));
                }
            }
        }

        [Command("togglewelcome"), Alias("togglew", "tw"),
         Summary("Toggles if welcome announcements are in or out of embeds")]
        public async Task ToggleWelcomeEmbed()
        {
            await _announcement.ToggleWelcomeEmbed(Context);
        }
        
        [Command("toggleleave"), Alias("togglel", "tl"),
         Summary("Toggles if leave announcements are in or out of embeds")]
        public async Task ToggleLeaveEmbed()
        {
            await _announcement.ToggleLeaveEmbed(Context);
        }
    }
}