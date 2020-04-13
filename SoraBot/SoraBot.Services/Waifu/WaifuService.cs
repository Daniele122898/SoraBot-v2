using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SoraBot.Data.Repositories.Interfaces;
using SoraBot.Services.Cache;

using WaifuDbo = SoraBot.Data.Models.SoraDb.Waifu;

namespace SoraBot.Services.Waifu
{
    public class WaifuService : IWaifuService
    {
        private readonly ICacheService _cacheService;
        private readonly IWaifuRepository _waifuRepo;

        private const string _WAIFU_CACHE_STRING = "wc";
        private const int _WAIFU_CACHE_TTL_MINS = 5;

        public WaifuService(ICacheService cacheService, IWaifuRepository waifuRepo)
        {
            _cacheService = cacheService;
            _waifuRepo = waifuRepo;
        }

        /// <summary>
        /// This method gets the WaifuList from the cache. If it does not exist in the cache
        /// it will create a DB request and then cache it for the next request
        /// </summary>
        /// <returns></returns>
        public async Task<List<WaifuDbo>> GetAllWaifus()
        {
            return await _cacheService.GetOrSetAndGetAsync<List<WaifuDbo>>(_WAIFU_CACHE_STRING,
                async () => await _waifuRepo.GetAllWaifus().ConfigureAwait(false),
                TimeSpan.FromMinutes(_WAIFU_CACHE_TTL_MINS)).ConfigureAwait(false);
        }
    }
}