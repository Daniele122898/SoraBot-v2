using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace SoraBot_v2.Services
{
    public static class CacheService
    {
        private static readonly ConcurrentDictionary<string, Item> _cacheDict = new ConcurrentDictionary<string, Item>();

        public const string DISCORD_SOCKET_MESSAGE = "discord::socketmessage::";

        public static async Task<SocketUserMessage> GetSocketMessage(ulong id, RequestOptions options = null)
        {
            string sId = DISCORD_SOCKET_MESSAGE + id;
            Object msgObj = Get(sId);
            SocketUserMessage msg = null;
            if (msgObj == null && options != null)
            {
                //Mesage isn't cached so check if request options are valid
                var channel = options.GetFrom as SocketTextChannel;
                if (channel != null)
                {
                    //If the message isn't cachged get it (if Request options are given)
                    msg = await SetDiscordSocketMessage(channel, id, options.Timeout);
                }
            }
            else
            {
                msg = msgObj as SocketUserMessage;
            }
            return msg;
        }

        public static async Task<SocketUserMessage> SetDiscordSocketMessage(SocketTextChannel channel, ulong msgId, TimeSpan timeout)
        {
            var msg = await channel.GetMessageAsync(msgId) as SocketUserMessage;
            if (msg == null)
                return null;
            Set(DISCORD_SOCKET_MESSAGE+msgId, new Item(msg, DateTime.UtcNow.Add(timeout)));
            return msg;
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

    public class RequestOptions
    {
        public Object GetFrom { get; }
        public TimeSpan Timeout { get; }

        public RequestOptions(Object getFrom, TimeSpan timeout)
        {
            GetFrom = getFrom;
            Timeout = timeout;
        }
    }

    public class Item
    {
        public Object Content { get; }
        public DateTime Timeout { get; }

        public Item(Object content, DateTime timeout)
        {
            Content = content;
            Timeout = timeout;
        }
    }
}