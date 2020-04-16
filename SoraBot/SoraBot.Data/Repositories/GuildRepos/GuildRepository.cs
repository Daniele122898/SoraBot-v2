using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SoraBot.Data.Repositories.Interfaces;

namespace SoraBot.Data.Repositories.GuildRepos
{
    public class GuildRepository : IGuildRepository
    {
        private readonly ITransactor<SoraContext> _soraTransactor;

        public GuildRepository(ITransactor<SoraContext> soraTransactor)
        {
            _soraTransactor = soraTransactor;
        }

        public async Task<string> GetGuildPrefix(ulong id)
            => await _soraTransactor.DoAsync(async context =>
                await context.Guilds.Where(g => g.Id == id).Select(x => x.Prefix).SingleOrDefaultAsync()
            ).ConfigureAwait(false);

        public Task<bool> SetGuildPrefix(ulong id, string prefix)
        {
            throw new System.NotImplementedException();
        }
    }
}