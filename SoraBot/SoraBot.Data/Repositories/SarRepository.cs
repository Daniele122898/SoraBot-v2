using System.Collections.Generic;
using System.Linq;
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

        public async Task<Maybe<List<Sar>>> GetAllSarsInGuild(ulong guildId)
            => await _soraTransactor.DoAsync(async context =>
            {
                var sars = await context.Sars
                    .Where(x => x.GuildId == guildId)
                    .ToListAsync()
                    .ConfigureAwait(false);
                if (sars.Count == 0) return Maybe.Zero<List<Sar>>();
                return Maybe.FromVal(sars);
            }).ConfigureAwait(false);
        
        public async Task AddSar(ulong roleId, ulong guildId)
            => await _soraTransactor.DoInTransactionAsync(async context =>
            {
                var sar = new Sar(roleId, guildId);
                context.Sars.Add(sar);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }).ConfigureAwait(false);


        public async Task RemoveSar(ulong roleId)
            => await _soraTransactor.DoInTransactionAsync(async context =>
            {
                var sar = await context.Sars.FindAsync(roleId).ConfigureAwait(false);
                if (sar == null) return;
                context.Sars.Remove(sar);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }).ConfigureAwait(false);
    }
}