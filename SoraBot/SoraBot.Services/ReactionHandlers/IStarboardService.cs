using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace SoraBot.Services.ReactionHandlers
{
    public interface IStarboardService
    {
        Task HandleReactionAdded(Cacheable<IUserMessage, ulong> msg, SocketReaction reaction);
        
        Task HandleReactionRemoved(Cacheable<IUserMessage, ulong> msg, SocketReaction reaction);
        
        Task HandleReactionCleared(Cacheable<IUserMessage, ulong> msg);
        
        
    }
}