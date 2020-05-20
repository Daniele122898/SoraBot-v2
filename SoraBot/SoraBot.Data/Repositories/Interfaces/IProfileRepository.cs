using System.Collections.Generic;
using System.Threading.Tasks;
using ArgonautCore.Lw;
using SoraBot.Data.Dtos.Profile;
using SoraBot.Data.Models.SoraDb;

namespace SoraBot.Data.Repositories.Interfaces
{
    public interface IProfileRepository
    {
        Task<Option<ProfileImageGenDto>> GetProfileStatistics(ulong userId, ulong guildId);
        Task SetUserHasBgBoolean(ulong userId, bool hasCustomBg);
        Task<Option<List<User>>> GetTop150Users();
        Task<Option<List<GuildUser>>> GetGuildUsersSorted(ulong guildId);
    }
}