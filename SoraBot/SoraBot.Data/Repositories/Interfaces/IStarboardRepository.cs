using System.Threading.Tasks;
using ArgonautCore.Maybe;

namespace SoraBot.Data.Repositories.Interfaces
{
    public interface IStarboardRepository
    {
        Task<Maybe<(ulong starboardChannelId, uint threshold)>> GetStarboardInfo(ulong guildId);
        Task SetStarboardChannleId(ulong guildId, ulong? starboardChannelId);
        Task SetStarboardThreshold(ulong guildId, uint threshold);
    }
}