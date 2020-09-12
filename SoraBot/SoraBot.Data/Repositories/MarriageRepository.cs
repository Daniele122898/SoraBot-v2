using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArgonautCore.Lw;
using Microsoft.EntityFrameworkCore;
using SoraBot.Data.Models.SoraDb;
using SoraBot.Data.Repositories.Interfaces;

namespace SoraBot.Data.Repositories
{
    public class MarriageRepository : IMarriageRepository
    {
        private readonly ITransactor<SoraContext> _soraTransactor;

        public MarriageRepository(ITransactor<SoraContext> soraTransactor)
        {
            _soraTransactor = soraTransactor;
        }

        public async Task<Option<List<Marriage>>> GetAllMarriagesOfUser(ulong userId)
            => await _soraTransactor.DoAsync(async context =>
            {
                var marriages = await context.Marriages.Where(x => x.Partner1 == userId || x.Partner2 == userId)
                    .ToListAsync().ConfigureAwait(false);

                if (marriages.Count == 0)
                    return Option.None<List<Marriage>>();
                
                return marriages;
            }).ConfigureAwait(false);

        public Task<bool> TryAddMarriage(ulong user1, ulong user2)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> TryDivorce(ulong user1, ulong user2)
        {
            throw new System.NotImplementedException();
        }
    }
}