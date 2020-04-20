using System;
using System.Threading.Tasks;
using ArgonautCore.Maybe;
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
        
        public Maybe<IUser> Get(ulong id)
        {
            var user = (IUser)_client.GetUser(id);
            if (user != null) return Maybe.FromVal(user);
            var userM = _cacheService.Get<IUser>(id);
            return userM;
        }

        public Maybe<IGuildUser> Get(ulong userId, ulong guildId)
        {
            var user = (IGuildUser)_client.GetGuild(guildId).GetUser(userId);
            if (user != null) return Maybe.FromVal(user);
            var userM = _cacheService.Get<IGuildUser>(userId.ToString() + guildId.ToString());
            return userM;
        }

        public async Task<Maybe<IUser>> GetOrSetAndGet(ulong id)
        {
            var user = this.Get(id);
            if (user.HasValue) return user;
            // Otherwise we gotta fetch and store it in cache
            var userResp = await _restClient.GetUserAsync(id).ConfigureAwait(false);
            if (userResp == null) return Maybe.Zero<IUser>();
            // Otherwise we save it
            _cacheService.Set(id, user, TimeSpan.FromMinutes(_USER_TTL_MINS));
            return Maybe.FromVal<IUser>(userResp);
        }

        public async Task<Maybe<IGuildUser>> GetOrSetAndGet(ulong userId, ulong guildId)
        {
            var user = this.Get(userId, guildId);
            if (user.HasValue) return user;
            // Do rest request
            var userResp = await _restClient.GetGuildUserAsync(guildId, userId).ConfigureAwait(false);
            if (userResp == null) return Maybe.Zero<IGuildUser>();
            // Otherwise set cache
            _cacheService.Set(userId.ToString() + guildId.ToString(), user, TimeSpan.FromMinutes(_USER_TTL_MINS));
            return Maybe.FromVal<IGuildUser>(userResp);
        }
    }
}