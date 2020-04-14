using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArgonautCore.Maybe;
using SoraBot.Data.Models.SoraDb;
using SoraBot.Services.Cache;
using WaifuDbo = SoraBot.Data.Models.SoraDb.Waifu;

namespace SoraBot.Services.Waifu
{
    public partial class WaifuService
    {
        public async Task<List<WaifuDbo>> GetAllWaifusFromUser(ulong userId)
            => await _waifuRepo.GetAllWaifusFromUser(userId).ConfigureAwait(false);

        public async Task<List<UserWaifu>> GetAllUserWaifus(ulong userId)
            => await _waifuRepo.GetAllUserWaifus(userId).ConfigureAwait(false);

        public async Task<Dictionary<WaifuRarity, int>> GetTotalWaifuRarityStats()
        {
            return await _cacheService.GetOrSetAndGetAsync(CustomCacheStringIDs.WAIFU_RARITY_STATISTICS, async () =>
            {
                var allWaifus = await this.GetAllWaifus().ConfigureAwait(false);

                return allWaifus.GroupBy(w => w.Rarity, (rarity, ws) => new {rarity, count = ws.Count()})
                    .ToDictionary(x=> x.rarity, x => x.count);
            }, TimeSpan.FromHours(1)).ConfigureAwait(false);
        }

        public async Task<Maybe<(uint waifusSold, uint coinAmount)>> SellDupes(ulong userId)
            => await _waifuRepo.SellDupes(userId);

        public Task<WaifuDbo> GetWaifuByName(string name)
        {
            throw new NotImplementedException();
        }
    }
}