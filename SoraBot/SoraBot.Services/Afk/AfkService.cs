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
        private readonly ICacheService _cacheService;
        private readonly IAfkRepository _afkRepo;
        private readonly TimeSpan _afkTtl = TimeSpan.FromSeconds(30);
        

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
                Title = $"{Formatter.UsernameDiscrim(user)} is currently AFK"
            };
            
            if (!string.IsNullOrWhiteSpace(afk.Some().Afk.Message))
                eb.WithDescription((~afk).Afk.Message);
            
            await textChannel.SendMessageAsync("", embed: eb.Build());
        }

        private async Task<Option<AfkCacheItem>> GetUserAfkThroughCache(ulong userId)
        {
            bool inCache = true;
            var cached = await _cacheService.TryGetOrSetAndGetAsync<Data.Models.SoraDb.Afk>(
                CacheId.GetAfkId(userId),
                async () =>
                {
                    var afk = await _afkRepo.GetUserAfk(userId).ConfigureAwait(false);
                    inCache = false;
                    return !afk ? null : afk.Some();
                }, _afkTtl);
            
            if (!cached)
                return Option.None<AfkCacheItem>();
            
            return new Option<AfkCacheItem>(new AfkCacheItem(cached.Some(), inCache));
        }
    }
}