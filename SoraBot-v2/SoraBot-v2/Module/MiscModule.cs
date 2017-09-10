using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
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

        [Command("choose"), Summary("Give sora a list of which he shall choose one.")]
        public async Task Choose(
            [Summary("List of which he shall choose: option1 | option 2 | option 3"), Remainder] string chooseFrom)
        {
            string[] choosing;
            if (chooseFrom.IndexOf("|", StringComparison.Ordinal) < 0)
            {
                choosing = new[] {chooseFrom};
            }
            else
            {
                choosing = chooseFrom.Split("|");
            }
            List<string> bestChoose = new List<string>();
            foreach (var s in choosing)
            {
                if(!string.IsNullOrWhiteSpace(s))
                    bestChoose.Add(s);
            }
            Random r = new Random();

            if (bestChoose.Count == 0)
            {
                await Context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                    Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Found nothing to choose from!"));
                return;
            }
            bool codeBlock = true;
            string chosen = bestChoose[r.Next(bestChoose.Count)].TrimStart();
            if (Regex.IsMatch(chosen, @":\w+:") || Regex.IsMatch(chosen, @"<:\w+:\d+>"))//TODO TURN :EMOTE: INTO <:EMOTE:ID>
                codeBlock = false;


            await Context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                Utility.PurpleEmbed, Utility.SuccessLevelEmoji[4], "").WithDescription($"🔍 I've chosen {(codeBlock ? "`":"")}{chosen}{(codeBlock ? "`":"")}"));
        }

        [Command("minecraft"), Alias("skin", "minecraftskin"), Summary("Get the skin of your minecraft avatar")]
        public async Task Minecraft([Summary("Name of your minecraft account")] string name)
        {
            var eb = new EmbedBuilder()
            {
                Color = Utility.PurpleEmbed,
                Author = new EmbedAuthorBuilder()
                {
                    IconUrl = Context.User.GetAvatarUrl() ?? Utility.StandardDiscordAvatar,
                    Name = Utility.GiveUsernameDiscrimComb(Context.User)
                },
                ImageUrl = $"https://minotar.net/body/{HttpUtility.UrlEncode(name)}/300.png",
            };

            await Context.Channel.SendMessageAsync("", embed: eb);
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
                          "Blank never loses.";
            });
            await ReplyAsync("", embed: eb);
        }
    }
}