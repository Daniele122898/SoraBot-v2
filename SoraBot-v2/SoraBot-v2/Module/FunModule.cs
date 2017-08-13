using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using SoraBot_v2.Data.Entities;
using SoraBot_v2.Services;

namespace SoraBot_v2.Module
{
    public class FunModule : ModuleBase<SocketCommandContext>
    {

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
        public async Task Door(params SocketUser[] Users)
        {
            if (Users.Length < 1)
            {
                await ReplyAsync("",
                    embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                        "You need to specify at least one person to show the door to"));
                return;
            }

            string showDoors = "";
            foreach (var socketUser in Users)
            {
                showDoors += $"{Utility.GiveUsernameDiscrimComb(socketUser)}, ";
            }
            showDoors = showDoors.Remove(showDoors.Length - 2);
            var eb = new EmbedBuilder()
            {
                Color = Utility.PurpleEmbed,
                Description = $"**{showDoors}** 👉🚪"
            };
            await ReplyAsync("", embed: eb);
        }

        [Command("google"), Summary("Googles shit for you")]
        public async Task GoogleForMeOnegaishimasu([Remainder] string google)
        {
            string search = google.Replace(" ", "%20");
            await ReplyAsync("",
                embed: Utility.ResultFeedback(Utility.BlueInfoEmbed, Utility.SuccessLevelEmoji[3], $"{($"Results for \"{google}\"".Length > 200 ? $"Results for \"{google}".Remove(200)+"...\"":$"Results for \"{google}\"")}")
                    .WithUrl($"https://lmgtfy.com/?q={search}"));
        }
        
    }
}