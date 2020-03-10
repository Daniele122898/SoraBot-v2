using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Discord;

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
            object msgObj = Get(sId);
            IUserMessage msg = null;
            if (msgObj == null && options != null)
            {
                //Mesage isn't cached so check if request options are valid
                if (options.GetFrom is ITextChannel channel)
                {
                    //If the message isn't cachged get it (if Request options are given)
                    msg = await SetDiscordUserMessage(channel, id, options.Timeout);
                }
            }
            else if (msgObj != null)
            {
                msg = msgObj as IUserMessage;
            }
            return msg;
        }

        public static bool RemoveUserMessage(ulong id)
        {
            return _cacheDict.TryRemove(DISCORD_USER_MESSAGE + id, out _);
        }

        public static async Task<IUserMessage> SetDiscordUserMessage(ITextChannel channel, ulong msgId, TimeSpan timeout)
        {
            var msg = await channel.GetMessageAsync(msgId) as IUserMessage;
            if (msg == null)
                return null;
            Set(DISCORD_USER_MESSAGE + msgId, new Item(msg, DateTime.UtcNow.Add(timeout)));
            return msg;
        }

        public static async Task<IUserMessage> TrySetDiscordUserMessage(ITextChannel channel, ulong msgId,
            TimeSpan timeout)
        {
            var msgobj = Get(DISCORD_USER_MESSAGE + msgId);
            //MESSAGE ISNT CACHED! cache it
            if (msgobj == null)
            {
                return (await SetDiscordUserMessage(channel, msgId, timeout));
            }
            return (msgobj as IUserMessage);
        }

        private static void Set(string id, Item item)
        {
            _cacheDict.AddOrUpdate(id, item, (key, oldValue) => item);
        }

        private static object Get(string id)
        {
            if (!_cacheDict.TryGetValue(id, out var item))
            {
                return null;
            }

            if (item.Timeout.CompareTo(DateTime.UtcNow) <= 0)
            {
                // first remove entry and then return null
                _cacheDict.TryRemove(id, out _);
                return null;
            }
            return  item.Content;
        }
    }

    public class RequestOptions
    {
        public object GetFrom { get; }
        public TimeSpan Timeout { get; }

        public RequestOptions(object getFrom, TimeSpan timeout)
        {
            GetFrom = getFrom;
            Timeout = timeout;
        }
    }

    public class Item
    {
        public object Content { get; }
        public DateTime Timeout { get; }

        public Item(object content, DateTime timeout)
        {
            Content = content;
            Timeout = timeout;
        }
    }
}