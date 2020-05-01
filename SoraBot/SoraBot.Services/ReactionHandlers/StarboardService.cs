using System;
using System.Linq;
using System.Threading.Tasks;
using ArgonautCore.Maybe;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using SoraBot.Common.Extensions.Modules;
using SoraBot.Common.Utils;
using SoraBot.Data.Repositories.Interfaces;
using SoraBot.Services.Cache;

namespace SoraBot.Services.ReactionHandlers
{
    public class StarboardService : IStarboardService
    {
        public const string STAR_EMOTE = "⭐";
        public string DoNotPostId(ulong messageId) => "star:" + messageId.ToString();
        public string UserMessageReactCountId(ulong messageId, ulong userId) => messageId.ToString() + userId.ToString();

        private readonly TimeSpan _messageCacheTtl = TimeSpan.FromMinutes(10);
        private readonly TimeSpan _postedMsgTtl = TimeSpan.FromHours(1);

        private readonly ICacheService _cache;
        private readonly IStarboardRepository _starRepo;
        private readonly ILogger<StarboardService> _log;

        public StarboardService(
            ICacheService cache,
            IStarboardRepository starRepo,
            ILogger<StarboardService> log)
        {
            _cache = cache;
            _starRepo = starRepo;
            _log = log;
        }

        private static bool IsStarEmote(IEmote emote)
            => emote.Name == STAR_EMOTE;

        public async Task HandleReactionRemoved(Cacheable<IUserMessage, ulong> msg, SocketReaction reaction)
        {
            if (!IsStarEmote(reaction.Emote)) return;
            // Abort if its in the "do not post again" cache
            if (_cache.Contains(DoNotPostId(msg.Id))) return;
            
            // Try get message
            var message = await TryGetMessageAndValidate(msg, reaction.UserId).ConfigureAwait(false);
            if (message == null) return;
            
            // Check if this is in a guild and not DMs
            if (!(message.Channel is IGuildChannel channel)) return;
            var guildInfo = await _starRepo.GetStarboardInfo(channel.GuildId).ConfigureAwait(false);
            // This means that either there is no guild in the DB or it has no starboard Channel ID
            if (!guildInfo.HasValue) return;
            
            // Check if still valid channel and if not remove the values from the DB
            var starboardChannel = await this
                .IsValidChannelAndRemoveIfNot(guildInfo.Value.starboardChannelId, channel.Guild).ConfigureAwait(false);
            if (starboardChannel == null) return;
            
            // Check if still above threshold so we just update the count
            var reactionCount = await GetReactionCount(message, reaction.Emote).ConfigureAwait(false);
            if (reactionCount >= guildInfo.Value.threshold)
            {
                await this.TryUpdatePostedMessage(message, starboardChannel, reactionCount).ConfigureAwait(false);
                return;                
            }
            // Below threshold so we remove it from the Starboard and add it to the list of
            // never to be added again messages. (at least during runtime)
            var starmsg = await _starRepo.GetStarboardMessage(message.Id).ConfigureAwait(false);
            // This means its not in the DB so we don't care about it essentially
            if (!starmsg.HasValue) 
                return;
            await this.RemoveStarboadMessage(message.Id, starmsg.Value.PostedMsgId, starboardChannel)
                .ConfigureAwait(false);
        }

        private async Task RemoveStarboadMessage(ulong messageId, ulong postedMessageId, ITextChannel starboardChannel)
        {
            // Remove it from DB and Cache :)
            await this.RemoveStarboardMessageFromCacheAndDb(messageId, postedMessageId).ConfigureAwait(false);
            // Physically remove the message now
            var postedMsg = await this.GetStarboardMessage(postedMessageId, starboardChannel).ConfigureAwait(false);
            if (!postedMsg.HasValue) return; // Msg doesn't exist anymore
            try
            {
                await postedMsg.Value.DeleteAsync().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _log.LogError(e, "Failed to remove starboard message");
            }
            // Add it to the cache to never be added again
            _cache.Set(DoNotPostId(messageId), null);
        }
        
        public async Task HandleReactionAdded(Cacheable<IUserMessage, ulong> msg, SocketReaction reaction)
        {
            if (!IsStarEmote(reaction.Emote)) return;
            // Abort if its in the "do not post again" cache
            if (_cache.Contains(DoNotPostId(msg.Id))) return;
            
            // Try get message
            var message = await TryGetMessageAndValidate(msg, reaction.UserId).ConfigureAwait(false);
            if (message == null) return;

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
            if (!await this.TryUpdatePostedMessage(message, starboardChannel, reactionCount).ConfigureAwait(false))
            {
                // Post the message
                var postedMsg = await this.PostAndCacheMessage(message, starboardChannel, reactionCount)
                    .ConfigureAwait(false);
                await _starRepo.AddStarboardMessage(channel.Guild.Id, message.Id, postedMsg.Id).ConfigureAwait(false);
            }
            
            // We've handled the users Reaction. Let's keep track of it. A user is only allowed to react to a message TWICE
            // This means he can add and remove the star. After that his actions will be ignored
            
        }
        
        private async Task<IUserMessage> TryGetMessageAndValidate(Cacheable<IUserMessage, ulong> msg, ulong reactionUserId)
        {
            var messageM = await this.GetOrDownloadMessage(msg).ConfigureAwait(false);
            if (!messageM.HasValue) return null;
            if (messageM.Value.Author.IsBot || messageM.Value.Author.IsWebhook) return null;
            if (reactionUserId == messageM.Value.Author.Id) return null;
            
            return messageM.Value;
        }

        private async Task<bool> TryUpdatePostedMessage(IUserMessage message, ITextChannel starboardChannel,
            int reactionCount)
        {
            var starmsg = await _starRepo.GetStarboardMessage(message.Id).ConfigureAwait(false);
            if (!starmsg.HasValue)
                return false;

            // Check if message still exists
            var starMessage = await this.GetStarboardMessage(starmsg.Value.PostedMsgId, starboardChannel)
                .ConfigureAwait(false);
            if (starMessage.HasValue)
            {
                // Update message
                await starMessage.Value
                    .ModifyAsync(x => { x.Content = $"**{reactionCount.ToString()}** {STAR_EMOTE}"; })
                    .ConfigureAwait(false);
            }
            else
            {
                // Remove the message from the cache and from the repo
                await this.RemoveStarboardMessageFromCacheAndDb(starmsg.Value.MessageId, starmsg.Value.PostedMsgId)
                    .ConfigureAwait(false);
            }

            return true;
        }

        private async Task<IUserMessage> PostAndCacheMessage(IUserMessage msg, ITextChannel starboardChannel,
            int reactionCount)
        {
            var eb = new EmbedBuilder()
            {
                Color = SoraSocketCommandModule.Purple,
                Author = new EmbedAuthorBuilder()
                {
                    IconUrl = msg.Author.GetAvatarUrl() ?? msg.Author.GetDefaultAvatarUrl(),
                    Name = Formatter.UsernameDiscrim(msg.Author)
                }
            };
            if (!TryAddImageAttachment(msg, eb)) // First check if there's an attached image
                if (!TryAddImageLink(msg, eb)) // Check if there's an image link
                    TryAddArticleThumbnail(msg, eb); // Is it a link?

            // Otherwise make a normal embed
            if (!string.IsNullOrWhiteSpace(msg.Content))
                eb.WithDescription(msg.Content);

            eb.AddField("Posted in", $"[#{msg.Channel.Name} (take me!)]({msg.GetJumpUrl()})");
            eb.WithTimestamp(msg.Timestamp);

            var postedMsg = await starboardChannel
                .SendMessageAsync($"**{reactionCount.ToString()}** {STAR_EMOTE}", embed: eb.Build())
                .ConfigureAwait(false);

            _cache.Set(postedMsg.Id, postedMsg, _postedMsgTtl);
            return postedMsg;
        }

        private static void TryAddArticleThumbnail(IUserMessage msg, EmbedBuilder eb)
        {
            var thumbnail = msg.Embeds.Select(x => x.Thumbnail).FirstOrDefault(x => x.HasValue);
            if (!thumbnail.HasValue) return;
            eb.WithImageUrl(thumbnail.Value.Url);
        }

        private static bool TryAddImageLink(IUserMessage msg, EmbedBuilder eb)
        {
            var imageEmbed = msg.Embeds.Select(x => x.Image).FirstOrDefault(x => x.HasValue);
            if (!imageEmbed.HasValue) return false;
            eb.WithImageUrl(imageEmbed.Value.Url);
            return true;
        }

        private static bool TryAddImageAttachment(IUserMessage msg, EmbedBuilder eb)
        {
            if (msg.Attachments.Count == 0) return false;
            var image = msg.Attachments.FirstOrDefault(x => !Helper.LinkIsNoImage(x.Url));
            if (image == null) return false;
            eb.WithImageUrl(image.Url);
            return true;
        }

        private async Task RemoveStarboardMessageFromCacheAndDb(ulong messageId, ulong postedMessageId)
        {
            _cache.TryRemove<object>(messageId);
            _cache.TryRemove<object>(postedMessageId);
            await _starRepo.RemoveStarboardMessage(messageId).ConfigureAwait(false);
        }

        private async Task<Maybe<IUserMessage>> GetStarboardMessage(ulong messageId, ITextChannel starboardChannel)
        {
            return await _cache.TryGetOrSetAndGetAsync(
                messageId,
                async () => await starboardChannel.GetMessageAsync(messageId, CacheMode.AllowDownload)
                    .ConfigureAwait(false) as IUserMessage,
                _postedMsgTtl).ConfigureAwait(false);
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

        public Task HandleReactionCleared(Cacheable<IUserMessage, ulong> msg)
        {
            throw new System.NotImplementedException();
        }
    }
}