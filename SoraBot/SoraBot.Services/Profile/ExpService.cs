using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoraBot.Data.Repositories.Interfaces;
using SoraBot.Services.Utils;

namespace SoraBot.Services.Profile
{
    public class UserExpGain
    {
        public DateTime LastExpGain { get; set; }
        public uint AdditionalExp { get; set; }
    }

    public class ExpService : IExpService
    {
        public static int CalculateLevel(float exp)
        {
            return (int)(0.15F * Math.Sqrt(exp));
        }

        public static uint CalculateNeededExp(int lvl)
        {
            return (uint)Math.Pow((lvl/0.15F), 2.0);
        }
        
        public static string GetGuildExpCacheId(ulong guildId, ulong userId)
            => $"{guildId.ToString()}:{userId.ToString()}";
        
        private readonly RandomNumberService _rand;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<ExpService> _log;
        private readonly Timer _timer;

        private readonly ConcurrentDictionary<ulong, UserExpGain> _expCache =
            new ConcurrentDictionary<ulong, UserExpGain>();
        
        private readonly ConcurrentDictionary<string, uint> _guildExpCache = new ConcurrentDictionary<string, uint>();

        
        private const uint _USER_EXP_GAIN = 10;
        private const int _USER_EXP_COOLDOWN_SECS = 10;
        private const int _USER_EXP_WRITEBACK_MIN = 60;
        private const int _USER_EXP_WRITEBACK_MAX = 240;

        public ExpService(RandomNumberService rand, IServiceScopeFactory serviceScopeFactory, ILogger<ExpService> log)
        {
            _rand = rand;
            _serviceScopeFactory = serviceScopeFactory;
            _log = log;
            _timer = new Timer(WriteBackCache, null, TimeSpan.FromSeconds(_USER_EXP_WRITEBACK_MIN),
                TimeSpan.FromSeconds(_USER_EXP_WRITEBACK_MAX));
        }

        private async void WriteBackCache(object state)
        {
            var sw = new Stopwatch();
            sw.Start();
            // Change it so we don't have weird interleavings when stuff is slower for once
            _timer.Change(TimeSpan.FromHours(1), TimeSpan.FromHours(1));
            try
            {
                _log.LogTrace("Started DB EXP Writeback.");
                // Writeback all the values
                var keys = _expCache.Keys;
                if (keys.Count == 0) return; // Wait for next period
                // Create a new scope so we can get proper DB context
                using var scope = _serviceScopeFactory.CreateScope();
                var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
                foreach (var key in keys)
                {
                    _expCache.TryRemove(key, out var wb);
                    if (wb == null) continue;
                    // Otherwise write new EXP into user DB
                    await userRepo.TryAddUserExp(key, wb.AdditionalExp).ConfigureAwait(false);
                }
                var guildRepo = scope.ServiceProvider.GetRequiredService<IGuildRepository>();
                var guildUserKeys = _guildExpCache.Keys;
                foreach (var key in guildUserKeys)
                {
                    _guildExpCache.TryRemove(key, out var wb);
                    var ids = key.Split(":");
                    await guildRepo.TryAddGuildUserExp(ulong.Parse(ids[0]), ulong.Parse(ids[1]), wb).ConfigureAwait(false);
                }                
            }
            catch (Exception e)
            {
                _log.LogError(e, "Unexpected error in EXP DB Writeback occured!");
            }
            finally
            {
                // At the end we'll set the next period randomly between min and max writeback delay
                var elapsed = sw.ElapsedMilliseconds;
                var secs = (int)(elapsed / 1000.0);
                int r = _rand.GetRandomNext(_USER_EXP_WRITEBACK_MIN, _USER_EXP_WRITEBACK_MAX) + secs;
                _log.LogTrace($"Finished DB EXP Writeback in {elapsed.ToString()} ms, next in {r.ToString()} seconds");
                _timer.Change(TimeSpan.FromSeconds(r), TimeSpan.FromSeconds(r));
            }
        }

        public Task TryGiveUserExp(SocketMessage msg, SocketGuildChannel channel)
        {
            var user = msg.Author;
            // Before we update anything we check if the user can gain again. 
            // We do this here instead of the addOrUpdate such that we do not incur
            // The high locking costs as well as needless memory write or access
            // We also use the same cooldown for Guild EXP as Global EXP. just makes life easier :)
            _expCache.TryGetValue(user.Id, out var userExpGain);
            if (userExpGain != null &&
                userExpGain.LastExpGain.AddSeconds(_USER_EXP_COOLDOWN_SECS) > DateTime.UtcNow)
            {
                // User cannot earn again so no need to update anything.
                return Task.CompletedTask;
            }

            _expCache.AddOrUpdate(
                user.Id,
                this.CreateNewExpItem(),
                UpdateExistingExpItm);
            
            // Also add GuildExp cache :D - No need to keep track of cooldown etc so much simpler cache ;)
            _guildExpCache.AddOrUpdate(
                GetGuildExpCacheId(channel.Guild.Id, user.Id),
                _USER_EXP_GAIN,
                (key, exp) => exp + _USER_EXP_GAIN);
            
            return Task.CompletedTask;
        }

        private static UserExpGain UpdateExistingExpItm(ulong key, UserExpGain exp)
        {
            exp.AdditionalExp += _USER_EXP_GAIN;
            exp.LastExpGain = DateTime.UtcNow;
            return exp;
        }

        private UserExpGain CreateNewExpItem()
            => new UserExpGain()
            {
                AdditionalExp = _USER_EXP_GAIN,
                LastExpGain = DateTime.UtcNow,
            };
    }
}