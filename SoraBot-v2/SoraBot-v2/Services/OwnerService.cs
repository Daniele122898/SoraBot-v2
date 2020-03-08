using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace SoraBot_v2.Services
{
    public class OwnerService
    {
        private ConcurrentBag<SocketGuild> _guildCache = new ConcurrentBag<SocketGuild>();

        private const double MAX_BOT_PERCENTAGE = 0.4;
        
        public async Task CollectBotServerInfoAndLeaveAfter(SocketCommandContext context)
        {
            if (!_guildCache.IsEmpty)
            {
                
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
                
                await guild.DownloadUsersAsync().ConfigureAwait(false);
                var users = guild.Users;
                int bots = users.Count(u => u.IsBot);
                double botPercentage = (double)bots / users.Count;
                if (botPercentage < MAX_BOT_PERCENTAGE)
                    continue;
                
                _guildCache.Add(guild);
            }

            if (_guildCache.IsEmpty)
            {
                
            }
        }

        public async Task LeaveCollectedBotServers()
        {
            
        }
    }
}