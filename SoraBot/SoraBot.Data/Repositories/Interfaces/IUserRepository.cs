using System.Threading.Tasks;
using ArgonautCore.Lw;
using SoraBot.Data.Models.SoraDb;

namespace SoraBot.Data.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<Some<User>> GetOrCreateUser(ulong id);
        Task<User> GetUser(ulong id);
        Task TryAddUserExp(ulong userId, uint expToAdd);
    }
}