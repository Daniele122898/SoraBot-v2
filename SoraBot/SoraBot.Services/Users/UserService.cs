using System;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using SoraBot.Services.Cache;

namespace SoraBot.Services.Users
{
    public class UserService : IUserService
    {
        private readonly ICacheService _cacheService;
        private readonly DiscordRestClient _restClient;
        private readonly DiscordSocketClient _client;

        private const int _USER_TTL_MINS = 60;
        
        public UserService(ICacheService cacheService, DiscordRestClient restClient, DiscordSocketClient client)
        {
            _cacheService = cacheService;
            _restClient = restClient;
            _client = client;
        }
        
        public IUser Get(ulong id)
        {
            var user = (IUser)_client.GetUser(id);
            if (user != null) return user;
            user = _cacheService.Get<IUser>(id);
            return user;
        }

        public IGuildUser Get(ulong userId, ulong guildId)
        {
            var user = (IGuildUser)_client.GetGuild(guildId).GetUser(userId);
            if (user != null) return user;
            user = _cacheService.Get<IGuildUser>(userId.ToString() + guildId.ToString());
            return user;
        }

        public async Task<IUser> GetOrSetAndGet(ulong id)
        {
            var user = this.Get(id);
            if (user != null) return user;
            // Otherwise we gotta fetch and store it in cache
            user = await _restClient.GetUserAsync(id).ConfigureAwait(false);
            if (user == null) return null;
            // Otherwise we save it
            _cacheService.Set(id, user, TimeSpan.FromMinutes(_USER_TTL_MINS));
            return user;
        }

        public async Task<IGuildUser> GetOrSetAndGet(ulong userId, ulong guildId)
        {
            var user = this.Get(userId, guildId);
            if (user != null) return user;
            // Do rest request
            user = await _restClient.GetGuildUserAsync(guildId, userId).ConfigureAwait(false);
            if (user == null) return null;
            // Otherwise set cache
            _cacheService.Set(userId.ToString() + guildId.ToString(), user, TimeSpan.FromMinutes(_USER_TTL_MINS));
            return user;
        }
    }
}