using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SoraBot_v2.Data;
using SoraBot_v2.Data.Entities.SubEntities;
using SoraBot_v2.Extensions;

namespace SoraBot_v2.Services
{
    
    /*
        Waifu trade
        ---------------
        
        Rarities
        ---------
        common				2000
        uncommon			800
        rare				200
        epic				20
        ultimate Waifu		1
        
        Waifu
        ---------
        id
        name
        imageurl
        rarity
        
        User Ownership
        -----------------
        id increment
        userid
        waifuId
        count
     */
    
    public class WaifuService
    {
        // TODO unbox, sell, maybe trade, maybe fav
        private List<Waifu> _boxCache = new List<Waifu>();
        
        public void Initialize()
        {
            // initial setup
            CreateRandomCache();
        }

        private void CreateRandomCache()
        {
            using (var soraContext = new SoraContext())
            {
                // get all waifus
                var waifus = soraContext.Waifus.ToArray();
                foreach (var waifu in waifus)
                {
                    // add each waifu * rarity amount to cache
                    int amount = GetRarityAmount(waifu.Rarity);
                    for (int i = 0; i < amount; i++)
                    {
                        _boxCache.Add(waifu);
                    }
                }
                // shuffle for some extra RNG
                _boxCache.Shuffle();
            }
        }

        private async Task<bool> GiveWaifuToId(ulong userId, int waifuId)
        {
            using (var soraContext = new SoraContext())
            {
                var userdb = Utility.GetOrCreateUser(userId, soraContext);
                
            }
            return true;
        }
        
        public async Task UnboxWaifu()
        {
            
        }

        private int GetRarityAmount(WaifuRarity rarity)
        {
            switch (rarity)
            {
                    case WaifuRarity.Common:
                        return 2000;
                    case WaifuRarity.Uncommon:
                        return 800;
                    case WaifuRarity.Rare:
                        return 200;
                    case WaifuRarity.Epic:
                        return 20;
                    case WaifuRarity.UltimateWaifu:
                        return 1;
            }
            return 0;
        }
    }
}