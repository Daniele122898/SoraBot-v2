using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Discord.WebSocket;
using SoraBot.Services.Utils;

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
        private readonly RandomNumberService _rand;
        private readonly Timer _timer;

        private readonly ConcurrentDictionary<ulong, UserExpGain> _expCache =
            new ConcurrentDictionary<ulong, UserExpGain>();

        private const uint _USER_EXP_GAIN = 10;
        private const int _USER_EXP_COOLDOWN_SECS = 10;
        private const int _USER_EXP_WRITEBACK_MIN = 60;
        private const int _USER_EXP_WRITEBACK_MAX = 240;

        public ExpService(RandomNumberService rand)
        {
            _rand = rand;
            _timer = new Timer(WriteBackCache, null, TimeSpan.FromSeconds(_USER_EXP_WRITEBACK_MIN),
                TimeSpan.FromSeconds(_USER_EXP_WRITEBACK_MAX));
        }

        private void WriteBackCache(object? state)
        {
            // Writeback all the values

            // At the end we'll set the next period randomly between min and max writeback delay
            int r = _rand.GetRandomNext(_USER_EXP_WRITEBACK_MIN, _USER_EXP_WRITEBACK_MAX);
            _timer.Change(TimeSpan.FromSeconds(r), TimeSpan.FromSeconds(r));
        }


        public Task TryGiveUserExp(SocketMessage msg, SocketGuildChannel channel)
        {
            var user = msg.Author;
            // Before we update anything we check if the user can gain again. 
            // We do this here instead of the addOrUpdate such that we do not incur
            // The high locking costs as well as needless memory write or access
            _expCache.TryGetValue(user.Id, out var userExpGain);
            if (userExpGain != null &&
                userExpGain.LastExpGain.AddSeconds(_USER_EXP_COOLDOWN_SECS) > DateTime.UtcNow)
            {
                // User cannot earn again so no need to update anything.
                return Task.CompletedTask;
            }

            _expCache.AddOrUpdate(
                user.Id,
                this.CreateNewExpItem(user.Id),
                UpdateExistingExpItm);

            return Task.CompletedTask;
        }

        private static UserExpGain UpdateExistingExpItm(ulong key, UserExpGain exp)
        {
            exp.AdditionalExp += _USER_EXP_GAIN;
            exp.LastExpGain = DateTime.UtcNow;
            return exp;
        }

        private UserExpGain CreateNewExpItem(ulong userId)
            => new UserExpGain()
            {
                AdditionalExp = _USER_EXP_GAIN,
                LastExpGain = DateTime.UtcNow,
                UserId = userId
            };
    }
}