using System.Threading.Tasks;
using ArgonautCore.Maybe;
using SoraBot.Data.Repositories.Interfaces;

namespace SoraBot.Data.Repositories
{
    public class StarboardRepository : IStarboardRepository
    {
        private readonly ITransactor<SoraContext> _soraTransactor;

        public StarboardRepository(ITransactor<SoraContext> soraTransactor)
        {
            _soraTransactor = soraTransactor;
        }
        
        public async Task<Maybe<(ulong starboardChannelId, uint threshold)>> GetStarboardInfo(ulong guildId)
        {
            return await _soraTransactor.DoAsync<Maybe<(ulong, uint)>>(async context =>
            {
                var guild = await context.Guilds.FindAsync(guildId).ConfigureAwait(false);
                if (guild?.StarboardChannelId == null) return Maybe.Zero<(ulong, uint)>();
                return Maybe.FromVal<(ulong, uint)>((guild.StarboardChannelId.Value, guild.StarboardThreshold));
            }).ConfigureAwait(false);
        }

        public async Task SetStarboardChannleId(ulong guildId, ulong? starboardChannelId)
            => await _soraTransactor.DoInTransactionAsync(async context =>
            {
                var guild = await context.Guilds.FindAsync(guildId).ConfigureAwait(false);
                if (guild == null) return;
                guild.StarboardChannelId = starboardChannelId;
                await context.SaveChangesAsync();
            }).ConfigureAwait(false);

        public async Task SetStarboardThreshold(ulong guildId, uint threshold)
            => await _soraTransactor.DoInTransactionAsync(async context =>
            {
                var guild = await context.Guilds.FindAsync(guildId).ConfigureAwait(false);
                if (guild == null) return;
                guild.StarboardThreshold = threshold;
                await context.SaveChangesAsync();
            }).ConfigureAwait(false);
    }
}