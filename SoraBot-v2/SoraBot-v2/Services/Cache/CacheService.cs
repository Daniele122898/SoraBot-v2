using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

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

        public static TReturn GetOrSet<TReturn>(string id, Func<TReturn> setAndGet, TimeSpan? ttl = null)
        {
            if (_cacheDict.TryGetValue(id, out var item) && item.Timeout.CompareTo(DateTime.UtcNow) > 0)
            {
                return (TReturn)item.Content;
            }
            // Otherwise use the get function to get it and set it
            TReturn result = setAndGet();
            if (!ttl.HasValue)
            {
                ttl = TimeSpan.FromHours(1);
            }
            // only set the cache if the result is not null
            if (result != null)
            {
                Set(id, new Item(result, DateTime.UtcNow.Add(ttl.Value)));
            } 
            return result;
        }
        
        public static async Task<TReturn> GetOrSet<TReturn>(string id, Func<Task<TReturn>> setAndGet, TimeSpan? ttl = null)
        {
            if (_cacheDict.TryGetValue(id, out var item) && item.Timeout.CompareTo(DateTime.UtcNow) > 0)
            {
                return (TReturn) item.Content;
            }
            // Otherwise use the get function to get it and set it
            TReturn result = await setAndGet().ConfigureAwait(false);
            if (!ttl.HasValue)
            {
                ttl = TimeSpan.FromHours(1);
            }
            // only set the cache if the result is not null
            if (result != null)
            {
                Set(id, new Item(result, DateTime.UtcNow.Add(ttl.Value)));
            } 
            return result;
        }
        
    }
}