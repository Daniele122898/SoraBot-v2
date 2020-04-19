using System;
using System.Threading.Tasks;
using Discord.WebSocket;
using SoraBot.Services.Cache;

namespace SoraBot.Services.Profile
{
    public class UserExpGain
    {
        public ulong UserId { get; set; }
        public DateTime LastExpGain { get; set; }
        public uint AdditionalExp { get; set; }
    }
    
    public class ExpService : IExpService
    {
        private readonly ICacheService _cacheService;

        private const string _USER_EXP_CACHE_ID = "exp:";
        private const uint _USER_EXP_GAIN = 10;
        private const int _USER_EXP_COOLDOWN_SECS = 10;
        
        public ExpService(ICacheService cacheService)
        {
            _cacheService = cacheService;
        }

        public async Task TryGiveUserExp(SocketMessage msg, SocketGuildChannel channel)
        {
            var user = msg.Author;
            // Before we update anything we check if the user can gain again. 
            // We do this here instead of the addOrUpdate such that we do not incur
            // The high locking costs as well as needless memory write or access
            var userExpGain = _cacheService.Get<UserExpGain>(_USER_EXP_CACHE_ID + user.Id.ToString());
            if (userExpGain.HasValue &&
                userExpGain.Value.LastExpGain.AddSeconds(_USER_EXP_COOLDOWN_SECS) > DateTime.UtcNow)
            {
                // User cannot earn again so no need to update anything.
                return;
            }
            
            _cacheService.AddOrUpdate(
                _USER_EXP_CACHE_ID + user.Id.ToString(), 
                this.CreateNewExpItem(user.Id),
                UpdateExistingExpItm);
        }

        private static CacheItem UpdateExistingExpItm(string key, CacheItem item)
        {
            var obj = (UserExpGain)item.Content;
            obj.AdditionalExp += _USER_EXP_GAIN;
            obj.LastExpGain = DateTime.UtcNow;
            return new CacheItem(obj, null);
        }

        private CacheItem CreateNewExpItem(ulong userId)
            => new CacheItem(new UserExpGain()
            {
                AdditionalExp = _USER_EXP_GAIN,
                LastExpGain = DateTime.UtcNow,
                UserId = userId
            }, null);
    }
}