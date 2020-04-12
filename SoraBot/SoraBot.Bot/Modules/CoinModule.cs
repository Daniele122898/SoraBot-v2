using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Humanizer;
using Humanizer.Localisation;
using Microsoft.Extensions.Logging;
using SoraBot.Common.Extensions.Modules;
using SoraBot.Common.Utils;
using SoraBot.Data.Repositories.Interfaces;

namespace SoraBot.Bot.Modules
{
    [Name("Coins")]
    [Group("Coins")]
    [Summary("All the commands for handling your Sora Coins")]
    public class CoinModule : SoraSocketCommandModule
    {
        public const short DAILY_COOLDOWN_HOURS = 20;
        public const uint DAILY_REWARD = 500;

        private readonly IUserRepository _userRepo;
        private readonly ICoinRepository _coinRepo;
        private readonly ILogger<CoinModule> _logger;

        public CoinModule(IUserRepository userRepo, ICoinRepository coinRepo, ILogger<CoinModule> logger)
        {
            _userRepo = userRepo;
            _coinRepo = coinRepo;
            _logger = logger;
        }

        [Command("daily")]
        [Alias("earn")]
        [Summary("Earn some Sora Coins every day.")]
        public async Task EarnDaily()
        {
            // First lets try and get OR create the user because we're gonna need him
            var userDb = await _userRepo.GetOrCreateUser(Context.User.Id);
            if (await FailedToGetUser(userDb)) return;

            var user = userDb.Value;
            var nextDailyPossible = user.LastDaily.AddHours(CoinModule.DAILY_COOLDOWN_HOURS);
            if (nextDailyPossible.CompareTo(DateTime.UtcNow) >= 0)
            {
                var timeRemaining = nextDailyPossible.Subtract(DateTime.UtcNow.TimeOfDay).TimeOfDay;
                await ReplyFailureEmbed(
                    $"You can't earn anymore right now. Please wait another {timeRemaining.Humanize(minUnit: TimeUnit.Second, precision: 2)}.");

                return;
            }

            // Otherwise we can earn
            if (await FailedTryTransaction(await _coinRepo.DoDaily(user.Id, DAILY_REWARD).ConfigureAwait(false)))
            {
                return;
            }

            await ReplySuccessEmbed(
                $"You gained {DAILY_REWARD} Sora Coins! You can earn again in {DAILY_COOLDOWN_HOURS}h.");
        }

        [Command("coins")]
        [Alias("bank")]
        [Summary("Check your own or someone else's Sora Coin balance")]
        public async Task GetCoinAmount([Summary("The @user you want to check. Leave blank to get your own balance")]
            IUser userT = null)
        {
            var user = userT ?? Context.User;
            // We dont care if the user exists. So we take the easy way out
            var amount = await _coinRepo.GetCoins(user.Id);

            await ReplyInfoEmbed(
                $"{(user.Id == Context.User.Id ? "You have" : $"{Formatter.UsernameDiscrim(user)} has")} {amount.ToString()} Sora Coins.");
        }
    }
}