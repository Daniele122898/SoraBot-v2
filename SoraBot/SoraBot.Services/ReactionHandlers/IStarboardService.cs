using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using SoraBot.Common.Messages.MessageAdapters;

namespace SoraBot.Services.ReactionHandlers
{
    public interface IStarboardService
    {
        Task HandleReactionAdded(Cacheable<IUserMessage, ulong> msg, ISocketMessageChannel channel,
            SocketReaction reaction);
        
        Task HandleReactionRemoved(Cacheable<IUserMessage, ulong> msg, ISocketMessageChannel channel,
            SocketReaction reaction);
        
        Task HandleReactionCleared(Cacheable<IUserMessage, ulong> msg, ISocketMessageChannel channel);
        
        
    }
}