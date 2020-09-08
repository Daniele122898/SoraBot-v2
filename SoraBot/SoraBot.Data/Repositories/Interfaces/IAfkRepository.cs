using System.Threading.Tasks;
using ArgonautCore.Lw;
using SoraBot.Data.Models.SoraDb;

namespace SoraBot.Data.Repositories.Interfaces
{
    public interface IAfkRepository
    {
        public Task<Option<Afk>> GetUserAfk(ulong userId);
        public Task RemoveUserAfk(ulong userId);
        public Task SetUserAfk(ulong userId, string message);
    }
}