using System.Threading.Tasks;
using ArgonautCore.Maybe;
using SoraBot.Data.Models.SoraDb;
using SoraBot.Data.Repositories.GuildRepos;
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
                var starboard = await context.Starboards.FindAsync(guildId).ConfigureAwait(false);
                if (starboard == null) return Maybe.Zero<(ulong, uint)>();
                return Maybe.FromVal<(ulong, uint)>((starboard.StarboardChannelId, starboard.StarboardThreshold));
            }).ConfigureAwait(false);
        }

        public async Task SetStarboardChannelId(ulong guildId, ulong starboardChannelId)
            => await _soraTransactor.DoInTransactionAsync(async context =>
            {
                // We have to create a guild if it doesnt exist, bcs the starboard is a weak entity.
                await GuildRepository.GetOrSetAndGetGuild(guildId, context).ConfigureAwait(false);

                // Then we try and get or create the starboard
                var starboard = await context.Starboards.FindAsync(guildId).ConfigureAwait(false);
                if (starboard == null)
                {
                    // Create it
                    starboard = new Starboard(guildId, starboardChannelId);
                    // ReSharper disable once MethodHasAsyncOverload
                    context.Starboards.Add(starboard);
                }
                else
                {
                    starboard.StarboardChannelId = starboardChannelId;
                }

                await context.SaveChangesAsync();
            }).ConfigureAwait(false);

        public async Task RemoveStarboard(ulong guildId)
            => await _soraTransactor.DoInTransactionAsync(async context =>
            {
                var starboard = await context.Starboards.FindAsync(guildId).ConfigureAwait(false);
                if (starboard == null) return;
                context.Starboards.Remove(starboard);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }).ConfigureAwait(false);

        /// <summary>
        /// Set's the starboard threshold. This function assumes that the starboard exists. If not
        /// it will just noop. No error will be thrown
        /// </summary>
        public async Task SetStarboardThreshold(ulong guildId, uint threshold)
            => await _soraTransactor.DoInTransactionAsync(async context =>
            {
                var starboard = await context.Starboards.FindAsync(guildId).ConfigureAwait(false);
                if (starboard == null) return;
                starboard.StarboardThreshold = threshold;
                await context.SaveChangesAsync();
            }).ConfigureAwait(false);

        public async Task AddStarboardMessage(ulong guildId, ulong messageId, ulong postedMessageId)
            => await _soraTransactor.DoInTransactionAsync(async context =>
            {
                // Since this is a weak entity the guild has to exist
                await GuildRepository.GetOrSetAndGetGuild(guildId, context).ConfigureAwait(false);
                // Actually create and add it
                context.StarboardMessages.Add(new StarboardMessage(messageId, postedMessageId, guildId));
                await context.SaveChangesAsync().ConfigureAwait(false);
            }).ConfigureAwait(false);

        /// <summary>
        /// This method actually assumes that the entry exists. If it is not found
        /// this method will just noop. No errors are thrown.  
        /// </summary>
        public async Task RemoveStarboardMessage(ulong messageId)
            => await _soraTransactor.DoInTransactionAsync(async context =>
            {
                var msg = await context.StarboardMessages.FindAsync(messageId).ConfigureAwait(false);
                if (msg == null) return;
                context.StarboardMessages.Remove(msg);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }).ConfigureAwait(false);

        public async Task<Maybe<StarboardMessage>> GetStarboardMessage(ulong messageId)
            => await _soraTransactor.DoAsync(async context =>
            {
                var msg = await context.StarboardMessages.FindAsync(messageId).ConfigureAwait(false);
                return msg == null ? Maybe.Zero<StarboardMessage>() : Maybe.FromVal(msg);
            }).ConfigureAwait(false);
    }
}