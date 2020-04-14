using System.Collections.Generic;
using System.Threading.Tasks;
using ArgonautCore.Maybe;
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
        Task<Maybe<(uint waifusSold, uint coinAmount)>> SellDupes(ulong userId);

        Task<Waifu> GetWaifuByName(string name);
        Task<Waifu> GetWaifuById(int id);
    }
}