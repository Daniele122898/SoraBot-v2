using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Humanizer;
using SoraBot_v2.Services;

namespace SoraBot_v2.Module
{
    [Name("Misc")]
    public class FunModule : ModuleBase<SocketCommandContext>
    {

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

        private string[] rpsChoose = new[] {"paper", "scissor", "rock"};

        [Command("8ball"), Alias("8b"), Summary("Ask and get an 8ball answer")]
        public async Task Ball([Summary("Question"), Remainder] string question)
        {
            Random r = new Random();
            await Context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                Utility.PurpleEmbed, Utility.SuccessLevelEmoji[4], "🎱 "+ball[r.Next(ball.Length)]).Build());
        }
        
        [Command("kawaii"), Alias("cute", "men", "man", "cuteness", "kawai"), Summary("What all men seek!")]
        public async Task Kawaii()
        {
            var eb = new EmbedBuilder()
            {
                ImageUrl = "https://i.imgur.com/gz7enYI.gif",
                Color = Utility.PurpleEmbed
            };
            await ReplyAsync("", embed: eb.Build());
        }


        [Command("rps"), Alias("rockpaperscissor"), Summary("Play rock paper scissor with Sora")]
        public async Task RPS([Remainder]string chose)
        {
            chose = chose.ToLower();
            Random r= new Random();
            string botRps = rpsChoose[r.Next(rpsChoose.Length)];
            
            switch (chose)
            {
                case ("paper"):
                    if (botRps == "rock")
                    {
                        //win
                        await Context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                            Utility.PurpleEmbed, Utility.SuccessLevelEmoji[4], "😀 You won!").WithDescription($"Sora chose `{botRps.Humanize()}`").Build());
                    }
                    else if (botRps == "paper")
                    {
                        //Draw
                        await Context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                            Utility.PurpleEmbed, Utility.SuccessLevelEmoji[4], "😐 It's a draw!").WithDescription($"Sora chose `{botRps.Humanize()}`").Build());
                    }
                    else
                    {
                        //lost
                        await Context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                            Utility.PurpleEmbed, Utility.SuccessLevelEmoji[4], "😢 You lost!").WithDescription($"Sora chose `{botRps.Humanize()}`").Build());
                        
                    }
                    break;
                case ("scissor"):
                case ("scissors"):
                    if (botRps == "rock")
                    {
                        //lost
                        await Context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                            Utility.PurpleEmbed, Utility.SuccessLevelEmoji[4], "😢 You lost!").WithDescription($"Sora chose `{botRps.Humanize()}`").Build());
                    }
                    else if (botRps == "paper")
                    {
                        //win
                        await Context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                            Utility.PurpleEmbed, Utility.SuccessLevelEmoji[4], "😀 You won!").WithDescription($"Sora chose `{botRps.Humanize()}`").Build());
                    }
                    else
                    {
                        //Draw
                        await Context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                            Utility.PurpleEmbed, Utility.SuccessLevelEmoji[4], "😐 It's a draw!").WithDescription($"Sora chose `{botRps.Humanize()}`").Build());
                    }
                    break;
                case ("rock"):
                case ("rocks"):
                    if (botRps == "rock")
                    {
                        //Draw
                        await Context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                            Utility.PurpleEmbed, Utility.SuccessLevelEmoji[4], "😐 It's a draw!").WithDescription($"Sora chose `{botRps.Humanize()}`").Build());
                    }
                    else if (botRps == "paper")
                    {
                        //lost
                        await Context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                            Utility.PurpleEmbed, Utility.SuccessLevelEmoji[4], "😢 You lost!").WithDescription($"Sora chose `{botRps.Humanize()}`").Build());
                    }
                    else
                    {
                        //win
                        await Context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                            Utility.PurpleEmbed, Utility.SuccessLevelEmoji[4], "😀 You won!").WithDescription($"Sora chose `{botRps.Humanize()}`").Build());
                    }
                    break;
                default:
                    await Context.Channel.SendMessageAsync("", embed:Utility.ResultFeedback(
                        Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "You must enter a valid RPS option").Build());
                    return;
            }
        }

        [Command("lenny"), Summary("Lenny's the Chat")]
        public async Task Lenny()
        {
            await ReplyAsync("( ͡° ͜ʖ ͡°)");
        }

        [Command("swag", RunMode = RunMode.Async), Summary("Swags the chat")]
        public async Task Swag()
        {
            var msg = await ReplyAsync("( ͡° ͜ʖ ͡°)>⌐■-■");
            await Task.Delay(1500);
            await msg.ModifyAsync(x => { x.Content = "( ͡⌐■ ͜ʖ ͡-■)"; });
        }

        [Command("door"), Summary("Shows the door to someone")]
        public async Task Door(params SocketUser[] users)
        {
            if (users.Length < 1)
            {
                await ReplyAsync("",
                    embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                        "You need to specify at least one person to show the door to").Build());
                return;
            }

            string showDoors = "";
            foreach (var socketUser in users)
            {
                showDoors += $"{Utility.GiveUsernameDiscrimComb(socketUser)}, ";
            }
            showDoors = showDoors.Remove(showDoors.Length - 2);
            var eb = new EmbedBuilder()
            {
                Color = Utility.PurpleEmbed,
                Description = $"**{showDoors}** 👉🚪"
            };
            await ReplyAsync("", embed: eb.Build());
        }

        [Command("google"), Summary("Googles shit for you")]
        public async Task GoogleForMeOnegaishimasu([Remainder] string google)
        {
            string search = google.Replace(" ", "%20");
            await ReplyAsync("",
                embed: Utility.ResultFeedback(Utility.BlueInfoEmbed, Utility.SuccessLevelEmoji[3], $"{($"Results for \"{google}\"".Length > 200 ? $"Results for \"{google}".Remove(200)+"...\"":$"Results for \"{google}\"")}")
                    .WithUrl($"https://lmgtfy.com/?q={search}").Build());
        }
        
    }
}