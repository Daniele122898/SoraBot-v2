using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace SoraBot.Services.Cache
{
    public partial class CacheService : ICacheService
    {
        private readonly ConcurrentDictionary<ulong, CacheItem> _discordCache = new ConcurrentDictionary<ulong, CacheItem>();
        private readonly ConcurrentDictionary<string, CacheItem> _customCache = new ConcurrentDictionary<string, CacheItem>();

        // I'm not sure if its a smart idea to remove all the pointers to a 
        // reoccurring event. I dont want it to be GC'd. Not sure if that is a
        // thing but i'll just leave it to be sure :)
        // ReSharper disable once NotAccessedField.Local
        private Timer _timer;

        private const short _CACHE_CLEAN_DELAY = 60;
        
        public CacheService()
        {
            _timer = new Timer(CleanCaches, null, TimeSpan.FromSeconds(_CACHE_CLEAN_DELAY),
                TimeSpan.FromSeconds(_CACHE_CLEAN_DELAY));
        }

        #region CleanCaches
        private void CleanCaches(object stateInfo)
        {
            this.ClearSpecificCache<string>(_customCache);
            this.ClearSpecificCache<ulong>(_discordCache);
        }

        private void ClearSpecificCache<T>(ConcurrentDictionary<T, CacheItem> cache)
        {
            var dKeys = cache.Keys;
            foreach (var key in dKeys)
            {
                if (!cache.TryGetValue(key, out var item)) continue;
                if (!item.IsValid())
                {
                    cache.TryRemove(key, out _);
                }
            }
        }
        #endregion

        #region Getters
        public object Get(ulong id)
        {
            _discordCache.TryGetValue(id, out var item);
            if (item == null) return null;
            if (item.IsValid()) return item;
            
            _discordCache.TryRemove(id, out _);
            return null;
        }

        public T Get<T>(ulong id) where T : class
        {
            _discordCache.TryGetValue(id, out var item);
            if (item == null) return null;
            if (item.IsValid()) return (T)item.Content;
            
            _discordCache.TryRemove(id, out _);
            return null;
        }
        #endregion

        // Here on the other hand we WILL throw. Because this is a MAJOR fuckup if the item
        // is not the actual type. Because that means we would set the same ID to a different
        // type which should generally just not happen. This is bad design and should be punished
        
        public T GetOrSetAndGet<T>(ulong id, Func<T> set, TimeSpan? ttl = null)
        {
            return this.GetOrSetAndGet(id, _discordCache, set, ttl);
        }

        public async Task<T> GetOrSetAndGetAsync<T>(ulong id, Func<Task<T>> set, TimeSpan? ttl = null)
        {
            return await GetOrSetAndGetAsync(id, _discordCache, set, ttl).ConfigureAwait(false);
        }

        public void Set(ulong id, object obj, TimeSpan? ttl = null)
        {
            var itemToStore = new CacheItem(obj, ttl.HasValue ? (DateTime?)DateTime.UtcNow.Add(ttl.Value) : null);
            _discordCache.AddOrUpdate(id, itemToStore, ((key, cacheItem) => itemToStore));
        }
        
        private async Task<TReturn> GetOrSetAndGetAsync<TCacheKey, TReturn>(
            TCacheKey id, ConcurrentDictionary<TCacheKey, CacheItem> cache,
            Func<Task<TReturn>> set, TimeSpan? ttl = null)
        {
            if (cache.TryGetValue(id, out var item) && item != null && item.IsValid())
            {
                return (TReturn)item.Content;
            }
            // Otherwise we have to set it
            TReturn result = await set().ConfigureAwait(false);
            if (result == null)
            {
                throw new ArgumentException("Result of the set function was null. This is NOT acceptable");
            }
            var itemToStore = new CacheItem(result, ttl.HasValue ? (DateTime?)DateTime.UtcNow.Add(ttl.Value) : null);
            cache.AddOrUpdate(id, itemToStore, ((key, cacheItem) => itemToStore));
            return (TReturn) itemToStore.Content;
        }

        private TReturn GetOrSetAndGet<TCacheKey, TReturn>(
            TCacheKey id, ConcurrentDictionary<TCacheKey, CacheItem> cache,
            Func<TReturn> set, TimeSpan? ttl = null)
        {
            if (cache.TryGetValue(id, out var item) && item != null && item.IsValid())
            {
                return (TReturn)item.Content;
            }
            // Otherwise we have to set it
            TReturn result = set();
            if (result == null)
            {
                throw new ArgumentException("Result of the set function was null. This is NOT acceptable");
            }
            var itemToStore = new CacheItem(result, ttl.HasValue ? (DateTime?)DateTime.UtcNow.Add(ttl.Value) : null);
            cache.AddOrUpdate(id, itemToStore, ((key, cacheItem) => itemToStore));
            return (TReturn) itemToStore.Content;
        }
    }
}