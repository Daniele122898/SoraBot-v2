using System.Threading.Tasks;
using ArgonautCore.Maybe;
using SoraBot.Data.Models.SoraDb;

namespace SoraBot.Data.Repositories.Interfaces
{
    public interface IStarboardRepository
    {
        Task<Maybe<(ulong starboardChannelId, uint threshold)>> GetStarboardInfo(ulong guildId);
        Task SetStarboardChannelId(ulong guildId, ulong starboardChannelId);
        Task RemoveStarboard(ulong guildId);
        Task SetStarboardThreshold(ulong guildId, uint threshold);
        Task AddStarboardMessage(ulong guildId, ulong messageId, ulong postedMessageId);
        Task RemoveStarboardMessage(ulong messageId);
        Task<Maybe<StarboardMessage>> GetStarboardMessage(ulong messageId);
    }
}