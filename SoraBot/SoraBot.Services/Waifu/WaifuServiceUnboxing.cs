﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SoraBot.Data.Models.SoraDb;
using SoraBot.Data.Repositories.Interfaces;
using SoraBot.Services.Cache;
using SoraBot.Services.Utils;
using WaifuDbo = SoraBot.Data.Models.SoraDb.Waifu;

namespace SoraBot.Services.Waifu
{
    public partial class WaifuService : IWaifuService
    {
        private readonly ICacheService _cacheService;
        private readonly IWaifuRepository _waifuRepo;
        private readonly RandomNumberService _rand;

        private const int _WAIFU_CACHE_TTL_MINS = 15;

        private const int _COMMON_CHANCE = 525;
        private const int _UNCOMMON_CHANCE = 355;
        private const int _RARE_CHANCE = 85;
        // private const int _EPIC_CHANCE = 27;
        private const int _ULTI_CHANCE = 8;

        public WaifuService(
            ICacheService cacheService, 
            IWaifuRepository waifuRepo,
            RandomNumberService rand)
        {
            _cacheService = cacheService;
            _waifuRepo = waifuRepo;
            _rand = rand;
        }

        public async Task<bool> TryGiveWaifusToUser(ulong userid, List<WaifuDbo> waifus, uint boxCost)
            => await _waifuRepo.TryUnboxWaifus(userid, waifus, boxCost).ConfigureAwait(false);

        /// <summary>
        /// This method gets the WaifuList from the cache. If it does not exist in the cache
        /// it will create a DB request and then cache it for the next request
        /// </summary>
        /// <returns></returns>
        public async Task<List<WaifuDbo>> GetAllWaifus()
        {
            return (await _cacheService.GetOrSetAndGetAsync<List<WaifuDbo>>((ulong) CustomCacheIDs.WaifuList,
                async () => await _waifuRepo.GetAllWaifus().ConfigureAwait(false),
                TimeSpan.FromMinutes(_WAIFU_CACHE_TTL_MINS)).ConfigureAwait(false)).Value;
        }

        public async Task<WaifuDbo> GetRandomSpecialWaifu(ulong userId, WaifuRarity specialRarity)
        {
            var specialList = (await this.GetAllWaifus().ConfigureAwait(false))
                .Where(x => x.Rarity == specialRarity).ToList();
            // Remove all waifus the user already has
            //  Creating a dictionary to avoid looping for then to hundreds of thousand of times :)
            var userWaifus =
                (await _waifuRepo.GetAllWaifusFromUserWithRarity(userId, specialRarity)
                    .ConfigureAwait(false)).ToDictionary(x=> x.Id, x=> true);
            var remaining = specialList.Where(w => !userWaifus.ContainsKey(w.Id)).ToList();
            
            return remaining[_rand.GetRandomNext(0, remaining.Count)];
        }

        public async Task<WaifuDbo> GetRandomWaifu()
        {
            var waifus = await this.GetAllWaifus().ConfigureAwait(false);
            if (waifus == null || waifus.Count == 0) return null;
            
            // First we get a random rarity
            var rarity = this.GetRandomRarity();
            // Get list of waifus with that rarity
            var listOfRarity = waifus.Where(x => x.Rarity == rarity).ToList();
            // pick random waifu from that list
            return listOfRarity[_rand.GetRandomNext(0, listOfRarity.Count)];

        }
        
        private WaifuRarity GetRandomRarity()
        {
            // get a number from 0 - 999
            int num = _rand.GetRandomNext(0, 1000);
            var chance = _COMMON_CHANCE;
            
            if (num < chance)
                return WaifuRarity.Common;

            chance += _UNCOMMON_CHANCE;
            if (num < chance)
                return WaifuRarity.Uncommon;
            
            chance += _ULTI_CHANCE;
            if (num < chance)
                return WaifuRarity.UltimateWaifu;

            chance += _RARE_CHANCE;
            if (num < chance)
                return WaifuRarity.Rare;

            return WaifuRarity.Epic;
        }
    }
}