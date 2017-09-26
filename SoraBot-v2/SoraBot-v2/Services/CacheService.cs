using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore.Query.ExpressionTranslators;

namespace SoraBot_v2.Services
{
    public static class CacheService
    {
        private static readonly ConcurrentDictionary<string, Item> _cacheDict = new ConcurrentDictionary<string, Item>();

        private static Timer _timer;

        private const int CACHE_DELAY = 60;
        
        public const string DISCORD_USER_MESSAGE = "discord::usermessage::";

        public static void Initialize()
        {
            _timer = new Timer(ClearCache, null, TimeSpan.FromSeconds(CACHE_DELAY),
                TimeSpan.FromSeconds(CACHE_DELAY));
        }

        private static void ClearCache(Object stateInfo)
        {
            Dictionary<string, Item> temp = new Dictionary<string, Item>(_cacheDict);
            foreach (var item in temp)
            {
                //timeout is earlier or equal to value
                if (item.Value.Timeout.CompareTo(DateTime.UtcNow) <= 0)
                {
                    //remove entry.
                    _cacheDict.TryRemove(item.Key, out _);
                }
            }
        }

        public static async Task<IUserMessage> GetUserMessage(ulong id, RequestOptions options = null)
        {
            string sId = DISCORD_USER_MESSAGE + id;
            Object msgObj = Get(sId);
            IUserMessage msg = null;
            if (msgObj == null && options != null)
            {
                //Mesage isn't cached so check if request options are valid
                var channel = options.GetFrom as ITextChannel;
                if (channel != null)
                {
                    //If the message isn't cachged get it (if Request options are given)
                    msg = await SetDiscordUserMessage(channel, id, options.Timeout);
                }
            }
            else if(msgObj != null)
            {
                msg = msgObj as IUserMessage;
            }
            return msg;
        }

        public static bool RemoveUserMessage(ulong id)
        {
            return _cacheDict.TryRemove(DISCORD_USER_MESSAGE+id, out _);
        }

        public static async Task<IUserMessage> SetDiscordUserMessage(ITextChannel channel, ulong msgId, TimeSpan timeout)
        {
            var msg = await channel.GetMessageAsync(msgId) as IUserMessage;
            if (msg == null)
                return null;
            Set(DISCORD_USER_MESSAGE+msgId, new Item(msg, DateTime.UtcNow.Add(timeout)));
            return msg;
        }

        private static void Set(string id, Item item)
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