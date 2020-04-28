using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace SoraBot.Services.ReactionHandlers
{
    public class StarboardService : IStarboardService
    {
        
        
        public Task HandleReactionAdded(Cacheable<IUserMessage, ulong> msg, ISocketMessageChannel channel, SocketReaction reaction)
        {
            throw new System.NotImplementedException();
        }

        public Task HandleReactionRemoved(Cacheable<IUserMessage, ulong> msg, ISocketMessageChannel channel, SocketReaction reaction)
        {
            throw new System.NotImplementedException();
        }

        public Task HandleReactionCleared(Cacheable<IUserMessage, ulong> msg, ISocketMessageChannel channel)
        {
            throw new System.NotImplementedException();
        }
    }
}