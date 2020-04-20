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
        Task<Maybe<uint>> QuickSellWaifu(ulong userId, int waifuId, uint amount, WaifuRarity? rarity = null);
        Task<UserWaifu> GetUserWaifu(ulong userId, int waifuId);
        Task<bool> SetUserFavWaifu(ulong userId, int waifuId);
        Task RemoveUserFavWaifu(ulong userId);
        Task<Maybe<Waifu>> GetFavWaifuOfUser(ulong userId);
        Task<bool> TryTradeWaifus(ulong offerUser, ulong wantUser, int offerWaifuId, int requestWaifuId);
        Task RemoveWaifu(int waifuId);
    }
}