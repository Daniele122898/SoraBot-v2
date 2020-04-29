using System.Linq;
using System.Threading.Tasks;
using ArgonautCore.Maybe;
using Microsoft.EntityFrameworkCore;
using SoraBot.Data.Models.SoraDb;
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

        public async Task<bool> SetGuildPrefix(ulong id, string prefix)
        {
            // let's at least test this
            if (string.IsNullOrWhiteSpace(prefix)) return false;
            return await _soraTransactor.TryDoInTransactionAsync(async context =>
            {
                var guild = await GetOrSetAndGetGuild(id, context).ConfigureAwait(false);
                guild.Prefix = prefix;
                await context.SaveChangesAsync().ConfigureAwait(false);
                return true;
            }).ConfigureAwait(false);
        }

        public async Task<Maybe<Guild>> GetOrSetAndGetGuild(ulong id)
            => await _soraTransactor.DoInTransactionAndGetAsync(async context
                => Maybe.FromVal(await GetOrSetAndGetGuild(id, context).ConfigureAwait(false))
            ).ConfigureAwait(false);

        public async Task<Guild> GetGuild(ulong id)
            => await _soraTransactor.DoAsync(async context
                => await context.Guilds.FindAsync(id).ConfigureAwait(false)
            ).ConfigureAwait(false);

        /// <summary>
        /// Tries to find a Guild and if it can't it'll create one and already save! 
        /// </summary>
        public static async Task<Guild> GetOrSetAndGetGuild(ulong id, SoraContext context)
        {
            var guild = await context.Guilds.FindAsync(id).ConfigureAwait(false);
            if (guild != null) return guild;
            // Create guild, save it and give it back
            guild = new Guild(id);
            // ReSharper disable once MethodHasAsyncOverload
            context.Guilds.Add(guild);
            await context.SaveChangesAsync().ConfigureAwait(false);
            return guild;
        }
    }
}