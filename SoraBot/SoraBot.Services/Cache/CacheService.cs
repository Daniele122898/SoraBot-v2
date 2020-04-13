using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace SoraBot.Services.Cache
{
    public class CacheService : ICacheService
    {
        private readonly ConcurrentDictionary<ulong, CacheItem> _discordCache = new ConcurrentDictionary<ulong, CacheItem>();
        private readonly ConcurrentDictionary<string, CacheItem> _customCache = new ConcurrentDictionary<string, CacheItem>();

        private Timer _timer;
        
        public CacheService()
        {
            
        }
        
        public object Get(string id)
        {
            throw new NotImplementedException();
        }

        public object Get(ulong id)
        {
            throw new NotImplementedException();
        }

        public T Get<T>(string id)
        {
            throw new NotImplementedException();
        }

        public T Get<T>(ulong id)
        {
            throw new NotImplementedException();
        }

        public T GetAndSet<T>(string id, Func<T> setAndGet, TimeSpan? ttl = null)
        {
            throw new NotImplementedException();
        }

        public T GetAndSet<T>(ulong id, Func<T> setAndGet, TimeSpan? ttl = null)
        {
            throw new NotImplementedException();
        }

        public Task<T> GetAndSetAsync<T>(string id, Func<T> setAndGet, TimeSpan? ttl = null)
        {
            throw new NotImplementedException();
        }

        public Task<T> GetAndSetAsync<T>(ulong id, Func<T> setAndGet, TimeSpan? ttl = null)
        {
            throw new NotImplementedException();
        }

        public void Set(string id, object obj, TimeSpan? ttl = null)
        {
            throw new NotImplementedException();
        }

        public void Set(ulong id, object obj, TimeSpan? ttl = null)
        {
            throw new NotImplementedException();
        }
    }
}