using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace SoraBot_v2.Services
{
    public class LogService
    {
        public bool LoggingGuildMessageCount { get; private set; } = false;
        
        private readonly ConcurrentDictionary<ulong, int> _guildMessageCount = new ConcurrentDictionary<ulong, int>();

        public void ToggleGuildMessageCount(DiscordSocketClient client)
        {
            LoggingGuildMessageCount = !LoggingGuildMessageCount;
            if (LoggingGuildMessageCount)
            {
                client.MessageReceived += LogGuildMessageCountOnMessageReceive;
            }
            else
            {
                client.MessageReceived -= LogGuildMessageCountOnMessageReceive;
            }
        }
        
        
        public Task LogGuildMessageCountOnMessageReceive(SocketMessage msg)
        {
            if (!this.LoggingGuildMessageCount)
            {
                return Task.CompletedTask;
            }

            // Ignore private channels for now
            if (!(msg.Channel is SocketGuildChannel channel))
            {
                return Task.CompletedTask;
            }

            // Will set first entry to 1 and otherwise update the entry with a +1
            _guildMessageCount.AddOrUpdate(channel.Guild.Id, 1, (key, oldValue) => oldValue + 1);
            
            return Task.CompletedTask;
        }

        public ConcurrentDictionary<ulong, int> GetLogCacheCopy()
        {
            return new ConcurrentDictionary<ulong, int>(_guildMessageCount);
        }

        public void StopLoggingAndClearCache()
        {
            this.LoggingGuildMessageCount = false;
            this._guildMessageCount.Clear();
        }
    }
}