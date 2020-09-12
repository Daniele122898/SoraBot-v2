using System.Collections.Generic;
using System.Threading.Tasks;
using ArgonautCore.Lw;
using SoraBot.Data.Models.SoraDb;

namespace SoraBot.Data.Repositories.Interfaces
{
    public interface IMarriageRepository
    {
        public Task<Option<List<Marriage>>> GetAllMarriagesOfUser(ulong userId);
        public Task<bool> TryAddMarriage(ulong user1, ulong user2);
        public Task<bool> TryDivorce(ulong user1, ulong user2);

    }
}