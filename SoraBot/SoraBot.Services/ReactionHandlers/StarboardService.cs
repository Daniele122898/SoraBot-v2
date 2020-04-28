using System;
using System.Threading.Tasks;
using ArgonautCore.Maybe;
using Discord;
using Discord.WebSocket;
using SoraBot.Services.Cache;

namespace SoraBot.Services.ReactionHandlers
{
    public class StarboardService : IStarboardService
    {
        private readonly TimeSpan _messageCacheTtl = TimeSpan.FromMinutes(10);

        private readonly ICacheService _cache;

        public StarboardService(ICacheService cache)
        {
            _cache = cache;
        }

        private static bool IsStarEmote(IEmote emote)
            => emote.Name == "⭐";

        public async Task HandleReactionAdded(Cacheable<IUserMessage, ulong> msg, SocketReaction reaction)
        {
            if (!IsStarEmote(reaction.Emote)) return;
            // Try get message
            var messageM = await this.GetOrDownloadMessage(msg).ConfigureAwait(false);
            if (!messageM.HasValue) return;
            var message = messageM.Value;
            // Check if this is in a guild and not DMs
            if (!(message.Channel is IGuildChannel channel)) return;
        }

        private async Task<Maybe<IUserMessage>> GetOrDownloadMessage(Cacheable<IUserMessage, ulong> msg)
            => await _cache.TryGetOrSetAndGetAsync(
                msg.Id,
                async () => await msg.GetOrDownloadAsync().ConfigureAwait(false),
                this._messageCacheTtl)
                .ConfigureAwait(false);

        public Task HandleReactionRemoved(Cacheable<IUserMessage, ulong> msg, SocketReaction reaction)
        {
            throw new System.NotImplementedException();
        }

        public Task HandleReactionCleared(Cacheable<IUserMessage, ulong> msg)
        {
            throw new System.NotImplementedException();
        }
    }
}