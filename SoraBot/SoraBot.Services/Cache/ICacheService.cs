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
        T Get<T>(string id) where T : class;
        T Get<T>(ulong id) where T : class;

        T GetOrSetAndGet<T>(string id, Func<T> set, TimeSpan? ttl = null);
        T GetOrSetAndGet<T>(ulong id, Func<T> set, TimeSpan? ttl = null);
        
        Task<T> GetOrSetAndGetAsync<T>(string id, Func<Task<T>> set, TimeSpan? ttl = null);
        Task<T> GetOrSetAndGetAsync<T>(ulong id, Func<Task<T>> set, TimeSpan? ttl = null);

        void Set(string id, object obj, TimeSpan? ttl = null);
        void Set(ulong id, object obj, TimeSpan? ttl = null);
    }
}