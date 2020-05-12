using System.Threading.Tasks;
using ArgonautCore.Lw;
using SoraBot.Data.Dtos.Profile;

namespace SoraBot.Data.Repositories.Interfaces
{
    public interface IProfileRepository
    {
        Task<Option<ProfileImageGenDto>> GetProfileStatistics(ulong userId, ulong guildId);
        Task SetUserHasBgBoolean(ulong userId, bool hasCustomBg);
    }
}