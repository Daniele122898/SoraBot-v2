using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using SoraBot_v2.Services;

namespace SoraBot_v2.Module
{
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        private CommandService _service;

        public HelpModule(CommandService service)
        {
            _service = service;
        }

        [Command("help"), Alias("h"), Summary("Provides Help")]
        public async Task Help([Remainder] string cmdName = null)
        {
            if (cmdName == null)
            {
                await Context.Channel.SendMessageAsync("", embed:
                    Utility.ResultFeedback(Utility.BlueInfoEmbed, Utility.SuccessLevelEmoji[3], "Click here for Wiki")
                        .WithUrl("http://git.argus.moe/serenity/SoraBot-v2/wikis/home")); 
                return;
            }

            var eb = new EmbedBuilder()
            {
                Color = Utility.BlueInfoEmbed,
                Title = $"{Utility.SuccessLevelEmoji[3]} Help for {cmdName}",
            };

            var found = false;
            foreach (var c in _service.Commands)
            {
                if (!c.Aliases.Contains(cmdName))
                    continue;

                eb.AddField((efb) =>
                {
                    efb.Name = c.Parameters.Aggregate(c.Name + "\n",
                        (current, cmd) => $"{current} {(cmd.IsOptional ? $"[<{cmd.Name}>]" : $"<{cmd.Name}>")}");
                    efb.Value =
                        c.Parameters.Aggregate(
                            c.Summary + "\n\n" +
                            c.Aliases.Aggregate("**Aliases**\n",
                                (current, alias) =>
                                    $"{current}{(c.Aliases.ElementAt(0) == alias ? string.Empty : ", ")}{alias}") +
                            "\n\n**Parameters** ",
                            (current, cmd) =>
                                $"{current}\n{cmd.Name} {(cmd.IsOptional ? "(optional)" : "")}: `{cmd.Summary}`");
                });
                found = true;
            }
            if (found)
            {
                await Context.Channel.SendMessageAsync("", embed: eb);
            }
            else
            {
                await Context.Channel.SendMessageAsync("", embed:
                    Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Couldn't find command!"));
            }
        }

    }
}