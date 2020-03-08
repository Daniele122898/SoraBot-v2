using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using SoraBot_v2.Extensions;

namespace SoraBot_v2.Services
{
    public class OwnerService
    {
        private readonly ConcurrentBag<SocketGuild> _guildCache = new ConcurrentBag<SocketGuild>();

        private readonly double _maxBotPercentage = 0.2;

        public OwnerService()
        {
            var maxConfig = ConfigService.GetConfigData("maxBotPercentage");
            if (string.IsNullOrWhiteSpace(maxConfig)) 
                return;
            // Invariant culture is important, otherwise it becomes 8 instead of 0.8...
            _maxBotPercentage = double.Parse(maxConfig, CultureInfo.InvariantCulture);
        }
        
        public async Task CollectBotServerInfoAndLeaveAfter(SocketCommandContext context)
        {
            if (!_guildCache.IsEmpty)
            {
                await LeaveCollectedBotServers(context);
                return;
            }

            var client = context.Client;
            var guilds = client.Guilds;
            Console.WriteLine($"Checking {guilds.Count} Guilds");
            int count = 0;
            foreach (var guild in guilds)
            {
                count++;
                if (count % 100 == 0)
                {
                    Console.WriteLine($"Checked {count}/{guilds.Count} guilds..");
                }
                
                // This ratelimites us
                // await guild.DownloadUsersAsync().ConfigureAwait(false);
                var users = guild.Users;
                int bots = users.Count(u => u.IsBot);
                double botPercentage = (double)bots / users.Count;
                if (botPercentage < _maxBotPercentage)
                    continue;
                
                _guildCache.Add(guild);
            }

            if (_guildCache.IsEmpty)
            {
                await context.ReplySoraEmbedSuccessResponse($"No Guilds had a Bot percentage higher than {_maxBotPercentage}");
                return;
            }

            // Prepare Json file with all the infos
            var guildInfos = _guildCache.Select(g => new
                {Id = g.Id, UserCount = g.Users.Count, BotCount = g.Users.Count(u => u.IsBot), Name = g.Name});
            var serialized = JsonConvert.SerializeObject(guildInfos, Formatting.Indented);
            string path = "guildTemp.json";
            await File.WriteAllTextAsync(path, serialized);
            await context.Channel.SendFileAsync(path, "I found these suspicious guilds. Call the method again to leave ALL of them");
            File.Delete(path);
        }

        private async Task LeaveCollectedBotServers(SocketCommandContext context)
        {
            Console.WriteLine($"Leaving {_guildCache.Count} Guilds");
            foreach (var guild in _guildCache)
            {
                await guild.LeaveAsync();
                await Task.Delay(TimeSpan.FromSeconds(1));
            }

            await context.ReplySoraEmbedSuccessResponse($"Left {_guildCache.Count} guilds");
            _guildCache.Clear();
        }
    }
}