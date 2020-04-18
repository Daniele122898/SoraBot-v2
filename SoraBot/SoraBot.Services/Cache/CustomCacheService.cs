using System;
using System.Threading.Tasks;

namespace SoraBot.Services.Cache
{
    public partial class CacheService
    {
        public object Get(string id)
        {
            _customCache.TryGetValue(id, out var item);
            if (item == null) return null;
            if (item.IsValid()) return item;
            
            _customCache.TryRemove(id, out _);
            return null;
        }

        public T Get<T>(string id) where T : class
        {
            _customCache.TryGetValue(id, out var item);
            if (item == null) return null;
            if (item.IsValid()) return (T)item.Content;
            
            _customCache.TryRemove(id, out _);
            return null;
        }

        public T GetOrSetAndGet<T>(string id, Func<T> set, TimeSpan? ttl = null)
        {
            return this.GetOrSetAndGet(id, _customCache, set, ttl);
        }

        public async Task<T> GetOrSetAndGetAsync<T>(string id, Func<Task<T>> set, TimeSpan? ttl = null)
        {
            return await GetOrSetAndGetAsync(id, _customCache, set, ttl).ConfigureAwait(false);
        }

        public void Set(string id, object obj, TimeSpan? ttl = null)
        {
            var itemToStore = new CacheItem(obj, ttl.HasValue ? (DateTime?)DateTime.UtcNow.Add(ttl.Value) : null);
            _customCache.AddOrUpdate(id, itemToStore, ((key, cacheItem) => itemToStore));
        }
    }
}