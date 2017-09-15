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