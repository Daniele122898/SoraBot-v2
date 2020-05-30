using System.Threading.Tasks;
using ArgonautCore.Lw;
using SoraBot.Data.Models.SoraDb;

namespace SoraBot.Data.Repositories.Interfaces
{
    public interface IGuildRepository
    {
        Task<string> GetGuildPrefix(ulong id);
        Task<bool> SetGuildPrefix(ulong id, string prefix);
        Task<Option<Guild>> GetOrSetAndGetGuild(ulong id);
        Task<Guild> GetGuild(ulong id);
        Task RemoveGuild(ulong id);
        Task TryAddGuildUserExp(ulong guildId, ulong userId, uint expToAdd);
    }
}