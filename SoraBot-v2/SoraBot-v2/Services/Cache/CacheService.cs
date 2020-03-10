using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace SoraBot_v2.Services
{
    public static partial class CacheService
    {
        public const string DISCORD_GUILD_PREFIX = "guild::prefix::";
        
        private static readonly ConcurrentDictionary<string, Item> _cacheDict = new ConcurrentDictionary<string, Item>();

        private static Timer _timer;
        private const int CACHE_DELAY = 60;

        private const string DISCORD_USER_MESSAGE = "usermessage::";

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
        
        public static void Set(string id, Item item)
        {
            _cacheDict.AddOrUpdate(id, item, (key, oldValue) => item);
        }

        public static object Get(string id)
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
}