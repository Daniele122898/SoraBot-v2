using System.Collections.Generic;
using System.Threading.Tasks;
using SoraBot.Data.Models.SoraDb;

namespace SoraBot.Data.Repositories.Interfaces
{
    public interface IWaifuRepository
    {
        Task<List<Waifu>> GetAllWaifus();
        Task<bool> TryUnboxWaifus(ulong userid, List<Waifu> waifus, uint boxCost);
    }
}