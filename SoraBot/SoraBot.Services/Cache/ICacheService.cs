using System;
using System.Threading.Tasks;

namespace SoraBot.Services.Cache
{
    /// <summary>
    /// Cache service. Has 2 different caches. One custom cache which users strings (slower)
    /// And one specifically for Discord snowflake IDs (faster)
    /// </summary>
    public interface ICacheService
    {
        object Get(string id);
        object Get(ulong id);
        T Get<T>(string id);
        T Get<T>(ulong id);

        T GetAndSet<T>(string id, Func<T> setAndGet, TimeSpan? ttl = null);
        T GetAndSet<T>(ulong id, Func<T> setAndGet, TimeSpan? ttl = null);
        
        Task<T> GetAndSetAsync<T>(string id, Func<T> setAndGet, TimeSpan? ttl = null);
        Task<T> GetAndSetAsync<T>(ulong id, Func<T> setAndGet, TimeSpan? ttl = null);

        void Set(string id, object obj, TimeSpan? ttl = null);
        void Set(ulong id, object obj, TimeSpan? ttl = null);
    }
}