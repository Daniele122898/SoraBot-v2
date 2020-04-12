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
        
        [Command("swag", RunMode = RunMode.Async), Summary("Swags the chat")]
        public async Task Swag()
        {
            var msg = await ReplyAsync("( ͡° ͜ʖ ͡°)>⌐■-■");
            await Task.Delay(1500);
            await msg.ModifyAsync(x => { x.Content = "( ͡⌐■ ͜ʖ ͡-■)"; });
        }
    }
}