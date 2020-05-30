using System;
using System.Threading.Tasks;
using ArgonautCore.Lw;

namespace SoraBot.Services.Cache
{
    public partial class CacheService
    {
        public Option<object> Get(string id)
        {
            _customCache.TryGetValue(id, out var item);
            if (item == null) return Option.None<object>();
            if (item.IsValid()) return Option.Some<object>(item);

            _customCache.TryRemove(id, out _);
            return Option.None<object>();
        }

        public Option<T> Get<T>(string id)
        {
            _customCache.TryGetValue(id, out var item);
            if (item == null) return Option.None<T>();
            if (item.IsValid()) return Option.Some<T>((T) item.Content);

            _customCache.TryRemove(id, out _);
            return Option.None<T>();
        }

        public bool Contains(string id) => _customCache.ContainsKey(id);

        public Option<T> GetOrSetAndGet<T>(string id, Func<T> set, TimeSpan? ttl = null)
        {
            return Option.Some(this.GetOrSetAndGet(id, _customCache, set, ttl));
        }

        public async Task<Option<T>> GetOrSetAndGetAsync<T>(string id, Func<Task<T>> set, TimeSpan? ttl = null)
        {
            return Option.Some<T>(await GetOrSetAndGetAsync(id, _customCache, set, ttl).ConfigureAwait(false));
        }

        public async Task<Option<T>> TryGetOrSetAndGetAsync<T>(string id, Func<Task<T>> set, TimeSpan? ttl = null)
        {
            return await this.TryGetOrSetAndGetAsync(id, _customCache, set, ttl).ConfigureAwait(false);
        }

        public void Set(string id, object obj, TimeSpan? ttl = null)
        {
            var itemToStore = new CacheItem(obj, ttl.HasValue ? (DateTime?) DateTime.UtcNow.Add(ttl.Value) : null);
            _customCache.AddOrUpdate(id, itemToStore, ((key, cacheItem) => itemToStore));
        }

        public void AddOrUpdate(string id, CacheItem addItem, Func<string, CacheItem, CacheItem> updateFunc)
        {
            this._customCache.AddOrUpdate(id, addItem, updateFunc);
        }

        public Option<T> TryRemove<T>(string id)
        {
            _customCache.TryRemove(id, out var cacheItem);
            if (cacheItem == null) return Option.None<T>();
            if (!cacheItem.IsValid()) return Option.None<T>();
            return Option.Some((T) cacheItem.Content);
        }

        public void TryRemove(string id)
        {
            _customCache.TryRemove(id, out _);
        }
    }
}