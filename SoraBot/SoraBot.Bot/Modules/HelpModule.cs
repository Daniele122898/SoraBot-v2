using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.EntityFrameworkCore.Internal;
using SoraBot.Common.Extensions.Modules;
using SoraBot.Services.Guilds;

namespace SoraBot.Bot.Modules
{
    [Name("Help")]
    [Summary("Commands to help with Sora and all his available commands")]
    public class HelpModule : SoraSocketCommandModule
    {
        private readonly CommandService _cmdService;
        private readonly IPrefixService _prefixService;

        public HelpModule(CommandService cmdService, IPrefixService prefixService)
        {
            _cmdService = cmdService;
            _prefixService = prefixService;
        }
        
        [Command("help"), Alias("h")]
        [Summary("Shows you all the categories of commands that exist in Sora")]
        public async Task Help()
        {
            string prefix = await _prefixService.GetPrefix(Context.Guild.Id).ConfigureAwait(false);
            var eb = new EmbedBuilder()
            {
                Color = Blue,
                Title = $"{InfoEmoji} Sora Help",
                Description = $"This shows you all the available categories. " +
                              $"You can use `{prefix}help <Category Name>` to get a list of all available commands " +
                              $"within a category. (without the <>)",
                Footer = RequestedByFooter(Context.User),
                ThumbnailUrl = Context.Client.CurrentUser.GetAvatarUrl()
            };

            foreach (var module in _cmdService.Modules)
            {
                if (string.IsNullOrWhiteSpace(module.Name)) continue;
                if (module.Preconditions.Any(p => p is RequireOwnerAttribute)) continue;
                int nrCmds = module.Commands.Count(c => !c.Preconditions.Any(p => p is RequireOwnerAttribute));
                eb.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = module.Name;
                    x.Value = $"{nrCmds.ToString()} commands";
                });
            }

            await ReplyAsync("", embed: eb.Build());
        }

        [Command("help"), Alias("h")]
        [Summary("Shows all the commands that are in a category")]
        public async Task HelpCategory([Summary("The name of the category"), Remainder]
            string category)
        {
            var module = _cmdService.Modules.FirstOrDefault(x =>
                x.Name.Equals(category.Trim(), StringComparison.OrdinalIgnoreCase));
            if (module == null)
            {
                await ReplyFailureEmbed("Couldn't find specified category. Make sure you spell it exactly right.");
                return;
            }
            string prefix = await _prefixService.GetPrefix(Context.Guild.Id).ConfigureAwait(false);
            var eb = new EmbedBuilder()
            {
                Title = $"{InfoEmoji} Help for {category.Trim()}",
                Color = Blue,
                Footer = RequestedByFooter(Context.User),
                ThumbnailUrl = Context.Client.CurrentUser.GetAvatarUrl()
            };

            string commands = module.Commands
                .Where(c => !c.Preconditions.Any(x => x is RequireOwnerAttribute))
                .Select(x => x.Name).Join(", ");

            string desc =
                $"Use `{prefix}help command <Command Name>` to get specific help  on a command. (without the <>)\n\n" +
                $"`{commands}`";

            eb.Description = desc;

            await ReplyAsync("", embed: eb.Build());
        }

        [Command("help command"), Alias("help cmd", "h cmd")]
        [Summary("Provides help for a specific command.")]
        [Priority(10)]
        public async Task HelpCommand([Remainder] string cmdName)
        {
            var commands = _cmdService.Commands
                .Where(x => x.Name.Equals(cmdName, StringComparison.OrdinalIgnoreCase))
                .ToList();
            if (commands.Count == 0)
            {
                await ReplyFailureEmbed("Could not find command. Make sure you spell it right!");
                return;
            }
            
            var eb = new EmbedBuilder()
            {
                Color = Blue,
                Title = $"{InfoEmoji} Help for {cmdName.Trim()}"
            };

            foreach (var cmd in commands)
            {
                string pars = cmd.Parameters.Select(x => $"<{x.Name}{(x.IsOptional ? "?" : "")}>").Join(" ");
                string desc = $"{cmd.Summary}\n";
                desc += cmd.Parameters.Select(x => $"**{x.Name}{(x.IsOptional ? " (optional)" : "")}:** {x.Summary}").Join("\n");
                eb.AddField(x =>
                {
                    x.Name = $"**{cmd.Name} {pars}**";
                    x.IsInline = false;
                    x.Value = desc;
                });
            }

            await ReplyAsync("", embed: eb.Build());
        }
    }
}