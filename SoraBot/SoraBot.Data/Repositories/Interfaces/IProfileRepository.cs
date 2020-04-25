using System.Threading.Tasks;
using ArgonautCore.Maybe;
using SoraBot.Data.Dtos.Profile;

namespace SoraBot.Data.Repositories.Interfaces
{
    public interface IProfileRepository
    {
        Task<Maybe<ProfileImageGenDto>> GetProfileStatistics(ulong userId, ulong guildId);
        Task SetUserHasBgBoolean(ulong userId, bool hasCustomBg);
    }
}