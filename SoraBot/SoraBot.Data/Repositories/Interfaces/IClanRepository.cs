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
        public Task<Option<List<User>>> GetClanMembers(int clanId, int? limit = null);
        public Task<int> GetMemberCount(int clanId);
        public Task<bool> DoesClanExistByName(string name);
        public Task<bool> IsUserInAClan(ulong userId);
        public Task<Option<Clan>> GetClanByUserId(ulong userId);
        public Task CreateClan(string name, ulong ownerId);
        public Task SetClanDescription(int clanId, string description);
        public Task SetClanAvatar(int clanId, string avatarUrl);
        public Task LevelUp(int clanId);
        public Task ChangeClanName(int clanId, string newName);
        public Task UserJoinClan(int clanId, ulong userId);
        public Task UserLeaveClan(ulong userId);
        public Task InviteUser(int clanId, ulong userId);
        public Task RemoveInvite(int clanId, ulong userId);

    }
}