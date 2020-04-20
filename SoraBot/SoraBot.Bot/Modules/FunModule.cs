using System;
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
        [Command("ruined"), Alias("dayruined")]
        [Summary("Posts the meme my disappointment is immeasurable")]
        public Task DayRuined() =>
            ReplyAsync("", embed: new EmbedBuilder()
            {
                ImageUrl = "https://i.imgur.com/pIddxrw.png",
                Color = Purple
            }.Build());
        
        [Command("swag"), Summary("Swags the chat")]
        public async Task Swag()
        {
            var msg = await ReplyAsync("( ͡° ͜ʖ ͡°)>⌐■-■");
            await Task.Delay(1500);
            await msg.ModifyAsync(x => { x.Content = "( ͡⌐■ ͜ʖ ͡-■)"; });
        }
        
        [Command("8ball"), Alias("8b"), Summary("Ask and get an 8ball answer")]
        public async Task Ball([Summary("Question"), Remainder] string question)
        {
            Random r = new Random();
            await ReplyDefaultEmbed("🎱 " + ball[r.Next(ball.Length)]);
        }

        #region data
        private string[] ball = new[]
        {
            "Signs point to yes. ",
            "Yes.",
            "Reply hazy, try again.",
            "Without a doubt. ",
            "My sources say no. ",
            "As I see it, yes. ",
            "You may rely on it.",
            "Concentrate and ask again.",
            "Outlook not so good. ",
            "It is decidedly so.",
            "Better not tell you now.",
            "Very doubtful. ",
            "Yes - definitely. ",
            "It is certain. ",
            "Cannot predict now. ",
            "Most likely. ",
            "Ask again later. ",
            "My reply is no. ",
            "Outlook good. ",
            "Don't count on it."
        };
        #endregion

    }
}