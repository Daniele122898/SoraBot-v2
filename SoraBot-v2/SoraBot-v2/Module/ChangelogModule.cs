using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using SoraBot_v2.Services;

namespace SoraBot_v2.Module
{
    [Name("Misc")]
    public class ChangelogModule : ModuleBase<SocketCommandContext>
    {
        [Command("changelog"), Alias("update"), Summary("Gives you a nice changelog <3")]
        public async Task GetChangelog()
        {
            var eb = new EmbedBuilder()
            {
                Color = Utility.PurpleEmbed,
                Title = $"Changelog v{Utility.SORA_VERSION}",
                Description = ChangelogService.GetChangelog(),
                Footer = Utility.RequestedBy(Context.User)
            };
            eb.AddField(x =>
            {
                x.IsInline = false;
                x.Name = "I'd love to hear your feedback";
                x.Value = $"[Join this guild to let me hear it]({Utility.DISCORD_INVITE})";
            });

            await ReplyAsync("", embed: eb.Build());
        }
    }
}