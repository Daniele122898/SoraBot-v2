using System.Threading.Tasks;
using SoraBot.Data.Repositories.Interfaces;
using SoraBot.Services.Cache;

namespace SoraBot.Services.Guilds
{
    public class PrefixService : IPrefixService
    {
        public const string CACHE_PREFIX = "prfx:";
        // public const short CACHE_TTL_MINS = 60;

        private readonly ICacheService _cacheService;
        private readonly IGuildRepository _guildRepo;

        public PrefixService(ICacheService cacheService, IGuildRepository guildRepo)
        {
            _cacheService = cacheService;
            _guildRepo = guildRepo;
        }

        public async Task<string> GetPrefix(ulong id)
        {
            string idStr = CACHE_PREFIX + id.ToString();
            return (await _cacheService.GetOrSetAndGetAsync(idStr,
                async () => await _guildRepo.GetGuildPrefix(id).ConfigureAwait(false) ?? "$"
            ).ConfigureAwait(false)).Value;
        }

        public async Task<bool> SetPrefix(ulong id, string prefix)
        {
            // Let's set it in the DB. And if it succeeds we'll also add it to our cache
            if (!await _guildRepo.SetGuildPrefix(id, prefix).ConfigureAwait(false))
                return false;
            // Update the Cache
            string idStr = CACHE_PREFIX + id.ToString();
            _cacheService.Set(idStr, prefix);
            return true;
        }
    }
}