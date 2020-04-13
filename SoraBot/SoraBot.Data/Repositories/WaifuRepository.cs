using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SoraBot.Data.Models.SoraDb;
using SoraBot.Data.Repositories.Interfaces;

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
        
    }
}