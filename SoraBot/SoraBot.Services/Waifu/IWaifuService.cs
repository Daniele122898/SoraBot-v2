using System.Collections.Generic;
using System.Threading.Tasks;
using WaifuDbo = SoraBot.Data.Models.SoraDb.Waifu;

namespace SoraBot.Services.Waifu
{
    public interface IWaifuService
    {
        Task<List<WaifuDbo>> GetAllWaifus();
        Task<WaifuDbo> GetRandomWaifu();
    }
}