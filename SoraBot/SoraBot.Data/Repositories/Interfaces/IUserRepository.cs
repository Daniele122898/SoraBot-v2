using System.Threading.Tasks;
using ArgonautCore.Maybe;
using SoraBot.Data.Models.SoraDb;

namespace SoraBot.Data.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<Maybe<User>> GetOrCreateUser(ulong id);
        Task<User> GetUser(ulong id);
        Task TryAddUserExp(ulong userId, uint expToAdd);
    }
}