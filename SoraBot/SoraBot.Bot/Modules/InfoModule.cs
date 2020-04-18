using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using SoraBot.Common.Extensions.Modules;

namespace SoraBot.Bot.Modules
{
    [Name("Info")]
    [Summary("Commands for general information about users or Sora")]
    public class InfoModule : SoraSocketCommandModule
    {
        [Command("avatar")]
        [Summary("Get the avatar of the @user or yourself if no one is tagged")]
        public async Task GetAvatar(
            [Summary("@User to get the avatar from, or no one to get your own")]
            SocketUser userT = null)
        {
            var user = userT ?? Context.User;
            var eb = new EmbedBuilder()
            {
                Footer  = RequestedByFooter(Context.User),
                ImageUrl = user.GetAvatarUrl(ImageFormat.Auto, 512) ?? user.GetDefaultAvatarUrl(),
                Color = Purple
            };
            await ReplyEmbed(eb);
        }
    }
}