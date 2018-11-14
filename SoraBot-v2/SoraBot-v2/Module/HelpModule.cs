using System.Collections.Generic;
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

        [Command("help2")]
        public async Task Help2()
        {
            
            Dictionary<string, List<CommandInfo>> cmds = new Dictionary<string, List<CommandInfo>>();
            int count = 0;
            foreach (var module in _service.Modules)
            {
                if(string.IsNullOrWhiteSpace(module.Name))
                    continue;
                if (!cmds.ContainsKey(module.Name))
                {
                    cmds.TryAdd(module.Name, new List<CommandInfo>());
                }

                cmds.TryGetValue(module.Name, out var cs);
                foreach (var command in module.Commands)
                {
                    if(command.Preconditions.Any(p => p is RequireOwnerAttribute))
                        continue;
                    cs.Add(command);
                    count++;
                }
            }
            var eb = new EmbedBuilder()
            {
                Color = Utility.BlueInfoEmbed,
                Title = Utility.SuccessLevelEmoji[3] + " Sora Commands",
                Description = $"This shows all {count} available commands. You can get further info by using `help commandName`",
                Footer = Utility.RequestedBy(Context.User)
            };
            
            foreach (KeyValuePair<string,List<CommandInfo>> pair in cmds)
            {
                string commands = "";
                foreach (var info in pair.Value)
                {
                    commands += $"`{info.Name}`, ";
                }

                commands = commands.Trim().TrimEnd(',');
                
                eb.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = pair.Key;
                    x.Value = commands;
                });
            }

            await ReplyAsync("", embed: eb.Build());
        }

        [Command("help"), Alias("h"), Summary("Provides Help")]
        public async Task Help([Remainder] string cmdName = null)
        {
            if (cmdName == null)
            {
                await Context.Channel.SendMessageAsync("", embed:
                    Utility.ResultFeedback(Utility.BlueInfoEmbed, Utility.SuccessLevelEmoji[3], "Click here for Wiki")
                        .WithUrl("https://github.com/Daniele122898/SoraBot-v2/wiki").Build()); 
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
                await Context.Channel.SendMessageAsync("", embed: eb.Build());
            }
            else
            {
                await Context.Channel.SendMessageAsync("", embed:
                    Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Couldn't find command!").Build());
            }
        }

    }
}