using System.Collections.Generic;
using System.Threading.Tasks;
using ArgonautCore.Lw;
using SoraBot.Data.Models.SoraDb;

namespace SoraBot.Data.Repositories.Interfaces
{
    public interface ISarRepository
    {
        Task<bool> CheckIfRoleAlreadyExists(ulong roleId);
        Task<Option<List<Sar>>> GetAllSarsInGuild(ulong guildId);
        Task AddSar(ulong roleId, ulong guildId);
        Task RemoveSar(ulong roleId);
    }
}