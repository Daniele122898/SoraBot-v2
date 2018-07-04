using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SoraBot_v2.Data;
using SoraBot_v2.Data.Entities;

namespace SoraBot_v2.Services
{
    public class BanService
    {
        // list of all banned users to minimize latency in CommandHandler
        private ConcurrentDictionary<ulong, bool> _bannedUsers = new ConcurrentDictionary<ulong, bool>();
        
        // Fetch list once from DB
        public void FetchBannedUsers()
        {
            using (var soraContext = new SoraContext())
            {
                var bans = soraContext.Bans.ToList() ?? new List<Ban>();
                if (bans.Count == 0)
                {
                    // if there are no bans we dont need to read anything.
                    return;
                }
                // Save Bans in local Dictionary for fast access cache
                foreach (var ban in bans)
                {
                    _bannedUsers.TryAdd(ban.UserId, true);
                }
            }
        }
        
        // Update incoming from webservice
        public void BanUserEvent(ulong id)
        {
            _bannedUsers.TryAdd(id, true);
        }
        
        // Ban user Command
        public async Task BanUser(ulong id)
        {
            if (!_bannedUsers.TryAdd(id, true))
            {
                return;
            }
            // TODO notify other shards to ban user
            
        }
        
        // check if  User Id is banned
        public bool IsBanned(ulong id)
        {
            if (_bannedUsers.ContainsKey(id))
            {
                // User is banned
                return true;
            }
            return false;
        }
    }
}