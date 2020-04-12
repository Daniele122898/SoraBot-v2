using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using SoraBot.Common.Extensions.Modules;

namespace SoraBot.Bot.Modules
{
    [Name("Fun")]
    [Summary("A bunch of fun and useless commands.")]
    public class FunModule : SoraSocketCommandModule
    {
        [Command("ruined")]
        [Alias("dayruined")]
        [Summary("Posts the meme my disappointment is immeasurable")]
        public Task DayRuined() =>
            ReplyAsync("", embed: new EmbedBuilder()
            {
                ImageUrl = "https://i.imgur.com/pIddxrw.png",
                Color = Purple
            }.Build());
    }
}