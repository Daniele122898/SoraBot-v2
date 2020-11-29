using System.Collections.Generic;
using System.Threading.Tasks;
using ArgonautCore.Lw;
using SoraBot.Data.Models.SoraDb;

namespace SoraBot.Data.Repositories.Interfaces
{
    public interface IClanRepository
    {
        public Task<Option<Clan>> GetClanById(int clanId);
        
        public Task<Option<Clan>> GetClanByName(string clanName);

        public Task<Option<List<User>>> GetClanMembers(int clanId);

        public Task<int> GetMemberCount(int clanId);
    }
}