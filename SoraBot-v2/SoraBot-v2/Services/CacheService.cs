using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace SoraBot_v2.Services
{
    public static class CacheService
    {
        private static ConcurrentDictionary<string, Item> _cacheDict = new ConcurrentDictionary<string, Item>();

        public const string DISCORD_SOCKET_MESSAGE = "discord::socketmessage::";

        public static bool TryGetSocketMessage(long id, out SocketUserMessage msg)
        {
            string sId = DISCORD_SOCKET_MESSAGE + id;
            Object msgObj = Get(sId);
            msg = msgObj as SocketUserMessage;
            return msg != null;
        }

        public static async Task<bool> SetDiscordSocketMessage(SocketTextChannel channel, ulong msgId, TimeSpan timout)
        {
            var msg = await channel.GetMessageAsync(msgId) as SocketUserMessage;
            if (msg == null)
                return false;
            Set(DISCORD_SOCKET_MESSAGE+msgId, new Item()
            {
                Content = msg,
                Timeout = DateTime.UtcNow.Add(timout)
            });
            return true;
        }

        public static void Set(string id, Item item)
        {
            _cacheDict.AddOrUpdate(id, item, (key, oldValue) => item);
        }

        private static Object Get(string id)
        {
            Item item;
            return _cacheDict.TryGetValue(id, out item) ? item.Content : null;
        }
    }

    public class Item
    {
        public Object Content { get; set; }
        public DateTime Timeout { get; set; }
    }
}