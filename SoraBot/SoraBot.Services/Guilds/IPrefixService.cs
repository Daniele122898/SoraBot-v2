using System.Threading.Tasks;

namespace SoraBot.Services.Guilds
{
    public interface IPrefixService
    {
        Task<string> GetPrefix(ulong id);
        Task<bool> SetPrefix(ulong id, string prefix);
    }
}