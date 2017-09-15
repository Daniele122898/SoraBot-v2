using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using SoraBot_v2.Services;

namespace SoraBot_v2.Module
{
    public class ModModule : ModuleBase<SocketCommandContext>
    {
        private ModService _modService;

        public ModModule(ModService modService)
        {
            _modService = modService;
        }

        [Command("ban"), Summary("Bans a user")]
        public async Task BanUser(SocketGuildUser user, [Remainder] string reason = null)
        {
            await _modService.BanUser(Context, user, reason);
        }

        [Command("kick"), Summary("Kicks a user")]
        public async Task KickUser(SocketGuildUser user, [Remainder] string reason = null)
        {
            await _modService.KickUser(Context, user, reason);
        }

        [Command("warn"), Summary("Warn a user")]
        public async Task WarnUser(SocketGuildUser user, [Remainder] string reason = null)
        {
            await _modService.WarnUser(Context, user, reason);
        }

        [Command("cases"), Alias("listcases"), Summary("Lists all cases a user has been part of")]
        public async Task Cases(SocketGuildUser user)
        {
            await _modService.ListAllCasesWithUser(Context, user);
        }

        [Command("rmwarn"), Alias("removewarning", "rmwarning"), Summary("Removes a warning")]
        public async Task RemoveWarning(SocketGuildUser user, int warnNr)
        {
            await _modService.RemoveWarnings(Context, user, warnNr, false);
        }

        [Command("rmallwarn"), Alias("removeallwarnings", "rmallwarnings", "rmwarns"),
         Summary("Removes all Warnings of user")]
        public async Task RemoveAllWarnigns(SocketGuildUser user)
        {
            await _modService.RemoveWarnings(Context, user, 0, true);
        }

        [Command("pardon"), Summary("Pardons a user and removes all his cases")]
        public async Task PardonUser(SocketGuildUser user)
        {
            //CHECK PERMS
            if (await _modService.CheckPermissions(Context, ModService.Case.Ban, Context.Guild.CurrentUser, user) == false)
                return;
            
            if (await _modService.PardonUser(user, Context.Guild, Context.User))
            {
                await ReplyAsync("", embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed,
                    Utility.SuccessLevelEmoji[0], "Successfully pardoned user"));
                return;
            }
            await ReplyAsync("", embed: Utility.ResultFeedback(Utility.RedFailiureEmbed,
                Utility.SuccessLevelEmoji[2], "Can't pardon user"));
        }

        [Command("reason"), Summary("Updates the reason on a case")]
        public async Task UpdateReason([Remainder]string reason)
        {
            var splitted = reason.Split(' ', 2);
            int caseNr;
            if (!int.TryParse(splitted[0], out caseNr))
            {
                await ReplyAsync("", embed: Utility.ResultFeedback(Utility.RedFailiureEmbed,
                    Utility.SuccessLevelEmoji[2], "Format is incorrect. Please follow the instructions!").WithDescription("the format is `caseNr YourReason`"));
                return;
            }
            await _modService.AddReason(Context, caseNr, splitted[1]);
        }

        [Command("punishlogs"), Alias("punish", "logs", "setlogs", "setpunish"), Summary("Sets the punishlogs channel")]
        public async Task SetPunishLogs(SocketTextChannel channelT = null)
        {
            var channel = (SocketTextChannel)(channelT ?? Context.Channel);
            await _modService.SetPunishLogsChannel(Context, channel);
        }

        [Command("rmpunishlogs"), Alias("rmpunish", "rmlogs"), Summary("Removes the punishlogs channel")]
        public async Task RemovePunishLogs()
        {
            await _modService.DeletePunishLogsChannel(Context);
        }
    }
}