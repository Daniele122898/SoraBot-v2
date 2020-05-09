using System.Threading.Tasks;
using ArgonautCore.Maybe;
using Microsoft.EntityFrameworkCore;
using SoraBot.Data.Models.SoraDb;
using SoraBot.Data.Repositories.Interfaces;

namespace SoraBot.Data.Repositories
{
    public class SarRepository : ISarRepository
    {
        private readonly ITransactor<SoraContext> _soraTransactor;

        public SarRepository(ITransactor<SoraContext> soraTransactor)
        {
            _soraTransactor = soraTransactor;
        }

        public async Task<bool> CheckIfRoleAlreadyExists(ulong roleId)
            => await _soraTransactor.DoAsync(async context =>
                await context.Sars.CountAsync(x => x.RoleId == roleId) == 1
            ).ConfigureAwait(false);

        public Task<Maybe<Sar>> GetAllSarsInGuild(ulong guildId)
        {
            throw new System.NotImplementedException();
        }

        public Task AddSar(ulong roleId, ulong guildId)
        {
            throw new System.NotImplementedException();
        }

        public Task RemoveSar(ulong roleId)
        {
            throw new System.NotImplementedException();
        }
    }
}