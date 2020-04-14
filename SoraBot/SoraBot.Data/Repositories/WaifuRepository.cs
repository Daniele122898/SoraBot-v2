using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArgonautCore.Maybe;
using Microsoft.EntityFrameworkCore;
using SoraBot.Data.Models.SoraDb;
using SoraBot.Data.Repositories.Interfaces;
using SoraBot.Data.Utils;

namespace SoraBot.Data.Repositories
{
    public class WaifuRepository : IWaifuRepository
    {
        private readonly ITransactor<SoraContext> _soraTransactor;

        public WaifuRepository(ITransactor<SoraContext> soraTransactor)
        {
            _soraTransactor = soraTransactor;
        }

        public async Task<List<Waifu>> GetAllWaifus()
            => await _soraTransactor
                .DoAsync(async context => await context.Waifus.ToListAsync().ConfigureAwait(false))
                .ConfigureAwait(false);

        public async Task<bool> TryUnboxWaifus(ulong userid, List<Waifu> waifus, uint boxCost)
        {
            return await _soraTransactor.TryDoInTransactionAsync(async context =>
            {
                var user = await context.Users.FindAsync(userid).ConfigureAwait(false);
                if (user == null) return false;
                
                // check if enough money
                if (user.Coins < boxCost) return false;
                // Remove money
                user.Coins -= boxCost;
                // Give waifus
                foreach (var waifu in waifus)
                {
                    var userWaifu = user.UserWaifus.FirstOrDefault(x => x.WaifuId == waifu.Id);
                    if (userWaifu != null)
                    {
                        userWaifu.Count++;
                        continue;
                    }
                    // otherwise we have to add it
                    userWaifu = new UserWaifu(userid, waifu.Id, 1);
                    user.UserWaifus.Add(userWaifu);
                }

                await context.SaveChangesAsync().ConfigureAwait(false);
                return true;
            }).ConfigureAwait(false);
        }

        public async Task<List<UserWaifu>> GetAllUserWaifus(ulong userId)
        {
            return await _soraTransactor.DoAsync(async context =>
            {
                return await context.Users.Where(u => u.Id == userId).SelectMany(x => x.UserWaifus).ToListAsync();
            }).ConfigureAwait(false);
        }

        public async Task<List<Waifu>> GetAllWaifusFromUser(ulong userId)
        {
            return await _soraTransactor.DoAsync(async context =>
            {
                return await context.Users.Where(u => u.Id == userId).SelectMany(x => x.UserWaifus)
                    .Select(w => w.Waifu).ToListAsync();
            }).ConfigureAwait(false);
        }

        public async Task<List<Waifu>> GetAllWaifusFromUserWithRarity(ulong userId, WaifuRarity rarity)
        {
            return await _soraTransactor.DoAsync(async context =>
            {
                return await context.Users.Where(u => u.Id == userId).SelectMany(x => x.UserWaifus)
                    .Select(w => w.Waifu).Where(y => y.Rarity == rarity).ToListAsync();
            }).ConfigureAwait(false);
        }

        public async Task<int> GetTotalWaifuCount()
        {
            return await _soraTransactor.DoAsync(async context =>
            {
                return await context.Waifus.CountAsync().ConfigureAwait(false);
            }).ConfigureAwait(false);
        }

        // TODO improve this code. this is rather slow and stupid ngl
        public async Task<Maybe<(uint waifusSold, uint coinAmount)>> SellDupes(ulong userId)
        {
            return await _soraTransactor.DoInTransactionAndGetAsync(async context =>
            {
                var dupes = await context.UserWaifus
                    .Where(x=> x.UserId == userId && x.Count > 1)
                    .ToListAsync()
                    .ConfigureAwait(false);
                
                if (dupes == null || dupes.Count == 0)
                    return Maybe.FromErr<(uint, uint)>("You don't have any dupes to sell! Open some Waifu Boxes");
                
                // Get the waifus that are not of special rarity
                var waifus = dupes.Select(d => d.Waifu).Where(w => !WaifuUtils.IsSpecialOrUltiWaifu(w.Rarity)).ToList();
                dupes = dupes.Where(d => waifus.Any(x => x.Id == d.WaifuId)).ToList();
                if (dupes.Count == 0)
                    return Maybe.FromErr<(uint, uint)>("You don't have any dupes to sell! Open some Waifu Boxes");
                // Remove the dupes and accumulate the coins
                uint totalCoins = 0;
                uint totalSold = 0;
                foreach (var dupe in dupes)
                {
                    uint sold = dupe.Count - 1;
                    totalSold += sold;
                    dupe.Count = 1;
                    totalCoins += sold * WaifuUtils.GetWaifuQuickSellCost(dupe.Waifu.Rarity);
                }

                var user = await context.Users.FindAsync(userId).ConfigureAwait(false);
                user.Coins += totalCoins;
                await context.SaveChangesAsync().ConfigureAwait(false);
                return Maybe.FromVal((totalSold, totalCoins));

            }).ConfigureAwait(false);
        }
    }
}