using System;
using System.Linq;
using System.Threading.Tasks;
using ArgonautCore.Maybe;
using Discord;
using Discord.WebSocket;
using SoraBot.Data.Models.SoraDb;
using SoraBot.Data.Repositories.Interfaces;
using SoraBot.Services.Cache;

namespace SoraBot.Services.ReactionHandlers
{
    public class StarboardService : IStarboardService
    {
        private readonly TimeSpan _messageCacheTtl = TimeSpan.FromMinutes(10);

        private readonly ICacheService _cache;
        private readonly IStarboardRepository _starRepo;

        public StarboardService(
            ICacheService cache,
            IStarboardRepository starRepo)
        {
            _cache = cache;
            _starRepo = starRepo;
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
            if (message.Author.IsBot || message.Author.IsWebhook) return;

            // Check if this is in a guild and not DMs
            if (!(message.Channel is IGuildChannel channel)) return;
            var guildInfo = await _starRepo.GetStarboardInfo(channel.GuildId).ConfigureAwait(false);
            // This means that either there is no guild in the DB or it has no starboard Channel ID
            if (!guildInfo.HasValue) return;

            // Check if still valid channel and if not remove the values from the DB
            var starboardChannel = await this
                .IsValidChannelAndRemoveIfNot(guildInfo.Value.starboardChannelId, channel.Guild).ConfigureAwait(false);
            if (starboardChannel == null) return;
            // Check threshold
            var reactionCount = await GetReactionCount(message, reaction.Emote).ConfigureAwait(false);
            if (reactionCount < guildInfo.Value.threshold) return;

            // Channel is setup and exists and msg exceed threshold.
            // Check if message is already posted
            var starmsg = await _starRepo.GetStarboardMessage(message.Id).ConfigureAwait(false);
            if (starmsg.HasValue)
            {
                // Check if message still exists
                var starMessage = await this.GetStarboardMessage(starmsg.Value.PostedMsgId, starboardChannel)
                    .ConfigureAwait(false);
                if (starMessage.HasValue)
                {                    
                    // Update message
                }
                else
                {
                    // Remove the message from the cache and from the repo
                    await this.RemoveStarboardMessageFromCacheAndDb(starmsg.Value.MessageId, starmsg.Value.PostedMsgId).ConfigureAwait(false);
                }
            }
            else
            {
                // Post the message
            }
        }

        private async Task PostAndCachceMessage(IUserMessage msg, ITextChannel starboardChannel)
        {
            
        }

        private async Task RemoveStarboardMessageFromCacheAndDb(ulong messageId, ulong postedMessageId)
        {
            _cache.TryRemove<IMessage>(messageId);
            _cache.TryRemove<IMessage>(postedMessageId);
            await _starRepo.RemoveStarboardMessage(messageId).ConfigureAwait(false);
        }

        private async Task<Maybe<IMessage>> GetStarboardMessage(ulong messageId, ITextChannel starboardChannel)
            => await _cache.TryGetOrSetAndGetAsync(
                messageId,
                async () => await starboardChannel.GetMessageAsync(messageId, CacheMode.AllowDownload)
                    .ConfigureAwait(false),
                _messageCacheTtl).ConfigureAwait(false);

        private async Task UpdatePostedMessage(StarboardMessage msg)
        {
        }

        private async Task<int> GetReactionCount(IUserMessage msg, IEmote emote)
        {
            var reactions = await msg.GetReactionUsersAsync(emote, 100).FlattenAsync().ConfigureAwait(false);
            return reactions.Count(u => u.Id != msg.Author.Id);
        }

        private async Task<ITextChannel> IsValidChannelAndRemoveIfNot(ulong channelId, IGuild guild)
        {
            var channel = await guild.GetTextChannelAsync(channelId, CacheMode.AllowDownload).ConfigureAwait(false);
            if (channel != null) return channel;
            // Otherwise get rid of outdated info in DB
            await _starRepo.RemoveStarboard(guild.Id).ConfigureAwait(false);
            return null;
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