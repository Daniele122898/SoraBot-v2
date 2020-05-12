using System;
using System.Threading.Tasks;
using ArgonautCore.Lw;
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
        
        public Option<IUser> Get(ulong id)
        {
            var user = (IUser)_client.GetUser(id);
            if (user != null) return Option.Some(user);
            var userM = _cacheService.Get<IUser>(CacheID.GetUser(id));
            return userM;
        }

        public Option<IGuildUser> Get(ulong userId, ulong guildId)
        {
            var user = (IGuildUser)_client.GetGuild(guildId).GetUser(userId);
            if (user != null) return Option.Some(user);
            var userM = _cacheService.Get<IGuildUser>(CacheID.GetGuildUser(userId, guildId));
            return userM;
        }

        public async Task<Option<IUser>> GetOrSetAndGet(ulong id)
        {
            var user = this.Get(id);
            if (user.HasValue) return user;
            // Otherwise we gotta fetch and store it in cache
            var userResp = await _restClient.GetUserAsync(id).ConfigureAwait(false);
            if (userResp == null) return Option.None<IUser>();
            // Otherwise we save it
            _cacheService.Set(CacheID.GetUser(id), user, TimeSpan.FromMinutes(_USER_TTL_MINS));
            return Option.Some<IUser>(userResp);
        }

        public async Task<Option<IGuildUser>> GetOrSetAndGet(ulong userId, ulong guildId)
        {
            var user = this.Get(userId, guildId);
            if (user.HasValue) return user;
            // Do rest request
            var userResp = await _restClient.GetGuildUserAsync(guildId, userId).ConfigureAwait(false);
            if (userResp == null) return Option.None<IGuildUser>();
            // Otherwise set cache
            _cacheService.Set(CacheID.GetGuildUser(userId, guildId), user, TimeSpan.FromMinutes(_USER_TTL_MINS));
            return Option.Some<IGuildUser>(userResp);
        }
    }
}