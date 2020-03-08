using System.IO;
using System.Threading.Tasks;
using Discord.Commands;
using Newtonsoft.Json;
using SoraBot_v2.Extensions;
using SoraBot_v2.Services;

namespace SoraBot_v2.Module
{
    public class LogModule : ModuleBase<SocketCommandContext>
    {
        private readonly LogService _logService;

        public LogModule(LogService logService)
        {
            _logService = logService;
        }

        [Command("toggleGuildLog")]
        [RequireOwner]
        public async Task ToggleGuildMessageCountLog()
        {
            _logService.ToggleGuildMessageCount(Context.Client);
            await this.ReplySoraEmbedSuccessResponse(
                $"Changed guild message count log to {_logService.LoggingGuildMessageCount.ToString()}");
        }

        [Command("stopandclearguildlog")]
        [RequireOwner]
        public async Task StopAndClearGuildLog()
        {
            _logService.StopLoggingAndClearCache();
            await this.ReplySoraEmbedSuccessResponse("Stopped logging and cleared cache");
        }

        [Command("getguildlog")]
        [RequireOwner]
        public async Task GetGuildMessageCountLog()
        {
            var cache = _logService.GetLogCacheCopy();
            
            var serialized = JsonConvert.SerializeObject(cache, Formatting.Indented);
            string path = "allGuildsCountTemp.json";
            await File.WriteAllTextAsync(path, serialized);
            await Context.Channel.SendFileAsync(path, "All the guilds message counts on this shard");
            File.Delete(path);
        }
    }
}