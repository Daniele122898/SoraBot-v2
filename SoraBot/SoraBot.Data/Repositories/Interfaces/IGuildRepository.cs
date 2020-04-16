using System.Threading.Tasks;

namespace SoraBot.Data.Repositories.Interfaces
{
    public interface IGuildRepository
    {
        Task<string> GetGuildPrefix(ulong id);
        Task<bool> SetGuildPrefix(ulong id, string prefix);
    }
}