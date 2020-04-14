﻿using System.Collections.Generic;
using System.Threading.Tasks;
using SoraBot.Data.Models.SoraDb;

namespace SoraBot.Data.Repositories.Interfaces
{
    public interface IWaifuRepository
    {
        Task<List<Waifu>> GetAllWaifus();
        Task<bool> TryUnboxWaifus(ulong userid, List<Waifu> waifus, uint boxCost);
        
        Task<List<UserWaifu>> GetAllUserWaifus(ulong userId);
        Task<List<Waifu>> GetAllWaifusFromUser(ulong userId);
        Task<List<Waifu>> GetAllWaifusFromUserWithRarity(ulong userId, WaifuRarity rarity);
        Task<int> GetTotalWaifuCount();
    }
}