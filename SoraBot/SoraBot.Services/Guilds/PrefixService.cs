using System.Threading.Tasks;
using SoraBot.Data.Repositories.Interfaces;
using SoraBot.Services.Cache;

namespace SoraBot.Services.Guilds
{
    public class PrefixService : IPrefixService
    {
        private readonly ICacheService _cacheService;
        private readonly IGuildRepository _guildRepo;

        public PrefixService(ICacheService cacheService, IGuildRepository guildRepo)
        {
            _cacheService = cacheService;
            _guildRepo = guildRepo;
        }

        public async Task<string> GetPrefix(ulong id)
        {
            return (await _cacheService.GetOrSetAndGetAsync(CacheId.PrefixCacheId(id),
                async () => await _guildRepo.GetGuildPrefix(id).ConfigureAwait(false) ?? "$"
            ).ConfigureAwait(false)).Some();
        }

        public async Task<bool> SetPrefix(ulong id, string prefix)
        {
            // Let's set it in the DB. And if it succeeds we'll also add it to our cache
            if (!await _guildRepo.SetGuildPrefix(CacheId.PrefixCacheId(id), prefix).ConfigureAwait(false))
                return false;
            // Update the Cache
            _cacheService.Set(id, prefix);
            return true;
        }
    }
}