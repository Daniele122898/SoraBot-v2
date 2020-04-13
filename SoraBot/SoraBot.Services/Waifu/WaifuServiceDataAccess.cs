using System.Collections.Generic;
using System.Threading.Tasks;
using SoraBot.Data.Models.SoraDb;
using WaifuDbo = SoraBot.Data.Models.SoraDb.Waifu;

namespace SoraBot.Services.Waifu
{
    public partial class WaifuService
    {
        public async Task<List<WaifuDbo>> GetAllWaifusFromUser(ulong userId)
            => await _waifuRepo.GetAllWaifusFromUser(userId).ConfigureAwait(false);

        public async Task<List<UserWaifu>> GetAllUserWaifus(ulong userId)
            => await _waifuRepo.GetAllUserWaifus(userId).ConfigureAwait(false);
    }
}