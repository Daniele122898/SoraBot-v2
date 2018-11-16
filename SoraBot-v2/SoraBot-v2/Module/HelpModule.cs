using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.EntityFrameworkCore.Internal;
using SoraBot_v2.Services;

namespace SoraBot_v2.Module
{
    [Name("Help")]
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

        [Command("help"), Alias("h"), Summary("Provides more help for a specific Category")]
        public async Task HelpCategory([Remainder] string category)
        {
            var module =
                _service.Modules.FirstOrDefault(x => x.Name.Equals(category, StringComparison.OrdinalIgnoreCase));
            if (module == null)
            {
                await ReplyAsync("", embed: Utility.ResultFeedback(
                    Utility.RedFailiureEmbed,
                    Utility.SuccessLevelEmoji[2], 
                    "Couldn't find specific Category. Make sure to spell it right!")
                .Build());
                return;
            }
            
            var eb = new EmbedBuilder()
            {
                Color = Utility.BlueInfoEmbed,
                Title = $"{Utility.SuccessLevelEmoji[3]} {module.Name} Help",
                Footer = Utility.RequestedBy(Context.User),
                ThumbnailUrl = Context.Client.CurrentUser.GetAvatarUrl()
            };
            string commands = "";
            foreach (var command in module.Commands)
            {
                commands += $"`{command.Name}`, ";
            }
            commands = commands.Trim().TrimEnd(',');
            eb.Description = "Use `help command commandName` to get specific Help on the command! Commands that show up twice have different parameters. [Check the wiki for more info!](https://github.com/Daniele122898/SoraBot-v2/wiki) \n\n"+commands;

            await ReplyAsync("", embed: eb.Build());
        }

        [Command("help"), Alias("h"), Summary("Shows all the categories and the wiki")]
        public async Task Help()
        {
            Dictionary<string, int> cmds = new Dictionary<string, int>();
            int count = 0;
            foreach (var module in _service.Modules)
            {
                if(string.IsNullOrWhiteSpace(module.Name))
                    continue;
                if(module.Preconditions.Any(p=> p is RequireOwnerAttribute))
                    continue;
                if (!cmds.ContainsKey(module.Name))
                {
                    cmds.TryAdd(module.Name, 0);
                }
                var num = cmds[module.Name];
                foreach (var command in module.Commands)
                {
                    if(command.Preconditions.Any(p => p is RequireOwnerAttribute))
                        continue;
                    num++;
                    count++;
                }
                cmds[module.Name] = num;
            }
            var eb = new EmbedBuilder()
            {
                Color = Utility.BlueInfoEmbed,
                Title = Utility.SuccessLevelEmoji[3] + " Sora Commands",
                Description = "This shows all the available categories. Use `help categoryName` to get all commands in that category. " +
                              "You can also use the Wiki which is very detailed and will provide the most help.",
                Footer = Utility.RequestedBy(Context.User),
                ThumbnailUrl = Context.Client.CurrentUser.GetAvatarUrl()
            };

            eb.AddField(x =>
            {
                x.IsInline = false;
                x.Name = "Wiki";
                x.Value = "[Click here to get to the Wiki](https://github.com/Daniele122898/SoraBot-v2/wiki)";
            });
            
            foreach (KeyValuePair<string,int> pair in cmds)
            {
                eb.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = pair.Key;
                    x.Value = $"{pair.Value} commands";
                });
            }

            await ReplyAsync("", embed: eb.Build());
        }
        
        
        [Command("help command"), Alias("h command", "h c"), Summary("Provides Help for a specific command."), Priority(10)]
        public async Task Help([Remainder] string cmdName)
        {
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
                    efb.Name = c.Parameters.Aggregate(c.Name + " ",
                        (current, cmd) => $"{current} {(cmd.IsOptional ? $"[<{cmd.Name}>]" : $"<{cmd.Name}>")}");
                    efb.Value = string.IsNullOrWhiteSpace(c.Summary) ? "No summary." : c.Summary;
                        /*c.Parameters.Aggregate(
                            c.Summary + "\n\n" +
                            c.Aliases.Aggregate("**Aliases**\n",
                                (current, alias) =>
                                    $"{current}{(c.Aliases.ElementAt(0) == alias ? string.Empty : ", ")}{alias}") +
                            "\n\n**Parameters** ",
                            (current, cmd) =>
                                $"{current}\n{cmd.Name} {(cmd.IsOptional ? "(optional)" : "")}: `{cmd.Summary}`");*/
                });

                eb.AddField(x =>
                {
                    x.IsInline = false;
                    x.Name = "Aliases";
                    x.Value = c.Aliases.Count == 0 ? "No aliases." : c.Aliases.Join(", ");
                });

                if (c.Parameters.Count != 0)
                {
                    eb.AddField(x =>
                    {
                        x.IsInline = false;
                        x.Name = "Parameters";
                        x.Value = c.Parameters.Aggregate("",(current, cmd)=>  $"{current}\n{cmd.Name} {(cmd.IsOptional ? "(optional)" : "")}: `{cmd.Summary}`");
                    });
                }
                
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