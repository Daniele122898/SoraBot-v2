using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using ArgonautCore.Lw;

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
        public Option<object> Get(ulong id)
        {
            if (!_discordCache.TryGetValue(id, out var item))
                return Option.None<object>();
            if (item.IsValid()) return Option.Some<object>(item.Content);
            
            _discordCache.TryRemove(id, out _);
            return Option.None<object>();
        }

        public Option<T> Get<T>(ulong id)
        {
            if (!_discordCache.TryGetValue(id, out var item))
                return Option.None<T>();
            if (item.IsValid()) return Option.Some((T)item.Content);
            
            _discordCache.TryRemove(id, out _);
            return Option.None<T>();
        }

        public bool Contains(ulong id) => _discordCache.ContainsKey(id);

        #endregion

        // Here on the other hand we WILL throw. Because this is a MAJOR fuckup if the item
        // is not the actual type. Because that means we would set the same ID to a different
        // type which should generally just not happen. This is bad design and should be punished
        
        public Option<T> GetOrSetAndGet<T>(ulong id, Func<T> set, TimeSpan? ttl = null)
        {
            return Option.Some(this.GetOrSetAndGet(id, _discordCache, set, ttl));
        }

        public async Task<Option<T>> GetOrSetAndGetAsync<T>(ulong id, Func<Task<T>> set, TimeSpan? ttl = null)
        {
            return Option.Some(await GetOrSetAndGetAsync(id, _discordCache, set, ttl).ConfigureAwait(false));
        }

        public async Task<Option<T>> TryGetOrSetAndGetAsync<T>(ulong id, Func<Task<T>> set, TimeSpan? ttl = null)
        {
            return await this.TryGetOrSetAndGetAsync(id, _discordCache, set, ttl).ConfigureAwait(false);
        }
        
        private async Task<Option<TReturn>> TryGetOrSetAndGetAsync<TCacheKey, TReturn>(
            TCacheKey id, ConcurrentDictionary<TCacheKey, CacheItem> cache,
            Func<Task<TReturn>> set, TimeSpan? ttl = null)
        {
            if (cache.TryGetValue(id, out var item) && item.IsValid())
            {
                return Option.Some((TReturn)item.Content);
            }
            // Otherwise we have to set it
            TReturn result = await set().ConfigureAwait(false);
            if (result == null)
            {
                return Option.None<TReturn>();
            }
            var itemToStore = new CacheItem(result, ttl.HasValue ? (DateTime?)DateTime.UtcNow.Add(ttl.Value) : null);
            cache.AddOrUpdate(id, itemToStore, ((key, cacheItem) => itemToStore));
            return Option.Some((TReturn) itemToStore.Content);
        }

        public void Set(ulong id, object obj, TimeSpan? ttl = null)
        {
            var itemToStore = new CacheItem(obj, ttl.HasValue ? (DateTime?)DateTime.UtcNow.Add(ttl.Value) : null);
            _discordCache.AddOrUpdate(id, itemToStore, ((key, cacheItem) => itemToStore));
        }

        public Option<T> TryRemove<T>(ulong id)
        {
            if (!_discordCache.TryRemove(id, out var cacheItem))
                return Option.None<T>();
            if (!cacheItem.IsValid()) return Option.None<T>();
            return Option.Some((T) cacheItem.Content);
        }

        public void TryRemove(ulong id)
        {
            _discordCache.TryRemove(id, out _);
        }

        public void AddOrUpdate(ulong id, CacheItem addItem, Func<ulong, CacheItem, CacheItem> updateFunc)
        {
            this._discordCache.AddOrUpdate(id, addItem, updateFunc);
        }
        
        private async Task<TReturn> GetOrSetAndGetAsync<TCacheKey, TReturn>(
            TCacheKey id, ConcurrentDictionary<TCacheKey, CacheItem> cache,
            Func<Task<TReturn>> set, TimeSpan? ttl = null)
        {
            if (cache.TryGetValue(id, out var item) && item.IsValid())
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
            if (cache.TryGetValue(id, out var item) && item.IsValid())
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