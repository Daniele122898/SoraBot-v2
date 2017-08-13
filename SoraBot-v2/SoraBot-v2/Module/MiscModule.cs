using System.Net.Sockets;
using System.Threading.Tasks;
using System.Xml.Linq;
using Discord;
using Discord.Commands;
using SoraBot_v2.Data;
using SoraBot_v2.Services;

namespace SoraBot_v2.Module
{
    public class MiscModule : ModuleBase<SocketCommandContext>
    {
        private SoraContext _soraContext;

        public MiscModule(SoraContext soraContext)
        {
            _soraContext = soraContext;
        }
        
        [Command("ping"), Summary("Gives the latency of the Bot to the Discord API")]
        public async Task Ping()
        {
            await ReplyAsync($"Pong! {Context.Client.Latency} ms :ping_pong:");
        }

        [Command("exc")]
        [RequireOwner]
        public async Task ThrowException()
        {
            int i2 = 0;
            int i = 10 / i2;
        }
        
        [Command("git"), Alias("gitlab", "github"), Summary("Posts the link to Github")]
        public async Task GithubPage()
        {
            await ReplyAsync("",
                embed: Utility.ResultFeedback(Utility.BlueInfoEmbed, Utility.SuccessLevelEmoji[3], "Click to View Sora's Github Repo").WithUrl("https://github.com/SubliminalHQ/Sora"));
        }

        [Command("invite"), Alias("inv"), Summary("Gives the invite Link to invite Sora")]
        public async Task InviteSora()
        {
            await ReplyAsync("",
                embed: Utility.ResultFeedback(Utility.BlueInfoEmbed, Utility.SuccessLevelEmoji[3], "Invite Sora to Your Guild")
                    .WithUrl("https://discordapp.com/oauth2/authorize?client_id=341935134787764226&scope=bot&permissions=305523831")
                    .WithDescription("Sora needs all the perms if you intend to use all of his features. Unchecking certain perms will inhibit some of Soras' functions\n" +
                                     "[Click to Invite](https://discordapp.com/oauth2/authorize?client_id=341935134787764226&scope=bot&permissions=305523831)"));
        }

        [Command("about"), Summary("Some info on Sora himself")]
        public async Task About()
        {
            var eb = new EmbedBuilder()
            {
                Color = Utility.BlueInfoEmbed,
                Title = $"{Utility.SuccessLevelEmoji[3]} About Sora",
                Footer = Utility.RequestedBy(Context.User),
                ThumbnailUrl = Context.Client.CurrentUser.GetAvatarUrl(),
                Description = $"Hei there (｡･ω･)ﾉﾞ\n" +
                              $"I was created by Serenity#0783. You can find him [here](https://discord.gg/Pah4yj5)"
            };

            eb.AddField(x =>
            {
                x.Name = "How was I created?";
                x.IsInline = false;
                x.Value = $"I was written in C# using the Discord.NET wrapper.\n" +
                          $"For more Info use `{Utility.GetGuildPrefix(Context.Guild, _soraContext)}info`\n" +
                          $"Or visit my [Github page](https://github.com/SubliminalHQ/Sora)";
            });

            eb.AddField(x =>
            {
                x.IsInline = false;
                x.Name = "About me";
                x.Value = "My name is Sora and I'm a member of Imanity.\n" +
                          "The last ranked exceed yet the strongest of em all.\n" +
                          "I'm currently 18 years old and my birth day is on the 3rd of June.\n" +
                          "I have a little but lovely Stepsister called Shiro. She and I together\n" +
                          "form the infamous duo 『　』also known as blank.\n" +
                          "Our next step is to conquer the world and challenge Tet.\n" +
                          "If you stand in our way we will have no other choice but\n" +
                          "to crush you. Because..\n" +
                          "Blank never looses.";
            });
            await ReplyAsync("", embed: eb);
        }
    }
}