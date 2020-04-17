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

        public async Task<WaifuDbo> GetWaifuByName(string name)
        {
            // Let's first try to get the cached waifu list and get the waifu from there
            // otherwise we gonna do a DB call to try get the waifu :)
            var cached = this.TryGetWaifuFromCache(w => w.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (cached.HasValue)
                return cached.Value;
            
            // The cache is empty so we gotta hit the db
            return await _waifuRepo.GetWaifuByName(name).ConfigureAwait(false);
        }

        public async Task<WaifuDbo> GetWaifuById(int id)
        {
            // Let's first try to get the cached waifu list and get the waifu from there
            // otherwise we gonna do a DB call to try get the waifu :)
            var cached = this.TryGetWaifuFromCache(w => w.Id == id);
            if (cached.HasValue)
                return cached.Value;
            
            // The cache is empty so we gotta hit the db
            return await _waifuRepo.GetWaifuById(id).ConfigureAwait(false);
        }

        public async Task<Maybe<uint>> TrySellWaifu(ulong userId, int waifuId, uint amount, WaifuRarity? rarity = null)
            => await _waifuRepo.QuickSellWaifu(userId, waifuId, amount, rarity);

        public async Task<UserWaifu> GetUserWaifu(ulong userid, int waifuId)
            => await _waifuRepo.GetUserWaifu(userid, waifuId).ConfigureAwait(false);

        public async Task<bool> SetUserFavWaifu(ulong userId, int waifuId)
            => await _waifuRepo.SetUserFavWaifu(userId, waifuId).ConfigureAwait(false);

        public async Task RemoveUserFavWaifu(ulong userId)
            => await _waifuRepo.RemoveUserFavWaifu(userId).ConfigureAwait(false);

        public async Task<bool> TryTradeWaifus(ulong offerUser, ulong wantUser, int offerWaifuId, int requestWaifuId)
            => await _waifuRepo.TryTradeWaifus(offerUser, wantUser, offerWaifuId, requestWaifuId).ConfigureAwait(false);

        public async Task RemoveWaifu(int waifuId)
            => await _waifuRepo.RemoveWaifu(waifuId).ConfigureAwait(false);
        


            /// <summary>
        /// Tries to get a waifu from the cache.
        /// This will return a Maybe with error if the cache is empty
        /// Otherwise it will return a maybe with either NULL if it couldn't be found or the waifu
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private Maybe<WaifuDbo> TryGetWaifuFromCache(Func<WaifuDbo, bool> predicate)
        {
            var waifuList = this._cacheService.Get<List<WaifuDbo>>((ulong) CustomCacheIDs.WaifuList);
            if (waifuList == null)
                return Maybe.FromErr<WaifuDbo>(string.Empty); // Error just means cache is empty
            return Maybe.FromVal(waifuList.FirstOrDefault(predicate)); // Here we pass a result even if there is none
        }
    }
}