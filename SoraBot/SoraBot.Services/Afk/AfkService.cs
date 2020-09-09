using System;
using System.Threading.Tasks;
using ArgonautCore.Lw;
using Discord;
using Discord.WebSocket;
using SoraBot.Common.Utils;
using SoraBot.Data.Repositories.Interfaces;
using SoraBot.Services.Cache;

using ss = SoraBot.Common.Extensions.Modules.SoraSocketCommandModule;

namespace SoraBot.Services.Afk
{
    public class AfkService : IAfkService
    {
        public static readonly TimeSpan AfkTtl = TimeSpan.FromSeconds(30);

        private readonly ICacheService _cacheService;
        private readonly IAfkRepository _afkRepo;
        

        public AfkService(ICacheService cacheService, IAfkRepository afkRepo)
        {
            _cacheService = cacheService;
            _afkRepo = afkRepo;
        }
        
        public async Task CheckUserAfkStatus(SocketGuildChannel channel, IUser user)
        {
            // Is this channel an actual text channel bcs otherwise we dont fucking care
            if (!(channel is SocketTextChannel textChannel))
                return;
            
            var afk = await this.GetUserAfkThroughCache(user.Id);
            if (!afk) return; // no afk status set

            if (afk.Some().InCache)
                return; // if it was present in the cache we dont want to respond to this afk request
            
            // Otherwise we respond
            var eb = new EmbedBuilder()
            {
                Color = ss.Purple,
                Title = $"💤 {Formatter.UsernameDiscrim(user)} is currently AFK"
            };
            
            if (!string.IsNullOrWhiteSpace(afk.Some().Afk.Message))
                eb.WithDescription((~afk).Afk.Message);
            
            await textChannel.SendMessageAsync("", embed: eb.Build());
        }

        private async Task<Option<AfkCacheItem>> GetUserAfkThroughCache(ulong userId)
        {
            // This user was checked and didnt have AFK so we return that. This is to stop the DB being queried for
            // every single mention that didnt have an AFK set.
            if (_cacheService.Contains(CacheId.GetAfkCheckId(userId)))
                return Option.None<AfkCacheItem>();

            bool inCache = true;
            var cached = await _cacheService.TryGetOrSetAndGetAsync<Data.Models.SoraDb.Afk>(
                CacheId.GetAfkId(userId),
                async () =>
                {
                    var afk = await _afkRepo.GetUserAfk(userId).ConfigureAwait(false);
                    inCache = false;
                    return !afk ? null : afk.Some();
                }, AfkTtl);

            if (!cached)
            {
                // TODO this is not clean at all and pretty bad practice. I just can't be bothered rn to write some form of hasmap to store the checks since i would need to make this singleton.
                _cacheService.Set(CacheId.GetAfkCheckId(userId), new object(), AfkTtl);
                return Option.None<AfkCacheItem>();
            }
            
            return new Option<AfkCacheItem>(new AfkCacheItem(cached.Some(), inCache));
        }
    }
}