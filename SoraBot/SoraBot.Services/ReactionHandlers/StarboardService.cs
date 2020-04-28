using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace SoraBot.Services.ReactionHandlers
{
    public class StarboardService : IStarboardService
    {
        private static bool IsStarEmote(IEmote emote)
            => emote.Name == "⭐";
        
        public async Task HandleReactionAdded(Cacheable<IUserMessage, ulong> msg, SocketReaction reaction)
        {
            if (!IsStarEmote(reaction.Emote)) return;
            // Try get message
            var message = await msg.GetOrDownloadAsync().ConfigureAwait(false);
            if (message == null) return;
            // Check if this is in a guild and not DMs
            if (!(message.Channel is IGuildChannel channel)) return;
                        
            
        }

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