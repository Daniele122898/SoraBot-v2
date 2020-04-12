using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Humanizer;
using Humanizer.Localisation;
using SoraBot.Common.Extensions.Modules;
using SoraBot.Common.Utils;
using SoraBot.Data.Models.SoraDb;
using SoraBot.Data.Repositories.Interfaces;

namespace SoraBot.Bot.Modules
{
    [Name("Coins")]
    [Summary("All the commands for handling your Sora Coins")]
    public class CoinModule : SoraSocketCommandModule
    {
        public const short DAILY_COOLDOWN_HOURS = 20;
        public const uint DAILY_REWARD = 500;

        private readonly IUserRepository _userRepo;
        private readonly ICoinRepository _coinRepo;

        public CoinModule(IUserRepository userRepo, ICoinRepository coinRepo)
        {
            _userRepo = userRepo;
            _coinRepo = coinRepo;
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
            var amount = _coinRepo.GetCoins(user.Id);

            await ReplyAsync("", embed:
                SimpleEmbed(Blue,
                        $"💰 {(user.Id == Context.User.Id ? "You have" : $"{Formatter.UsernameDiscrim(user)} has")} {amount.ToString()} Sora Coins.")
                    .Build());
        }

        [Command("send")]
        [Alias("transfer", "give")]
        [Summary("Send Sora Coins to another user.")]
        public async Task SendSoraCoins([Summary("The positive amount of Sora Coins to send to the user")]
            int amount,
            [Summary("The @user to send the SC to")]
            IUser user) => await SendMoney(user.Id, amount);

        [Command("send")]
        [Alias("transfer", "give")]
        [Summary("Send Sora Coins to another user.")]
        public async Task SendSoraCoins([Summary("The positive amount of Sora Coins to send to the user")]
            int amount,
            [Summary("The User ID to send the SC to")]
            ulong userId) => await SendMoney(userId, amount);

        private async Task SendMoney(ulong receiverId, int amount)
        {
            // Check if its a valid amount
            if (amount < 1)
            {
                await ReplyFailureEmbed("You must specify an amount greater than 0!");
                return;
            }

            // BEFORE we do ANYTHING with the receiver we check if the user even has enough money
            var userCoins = _coinRepo.GetCoins(Context.User.Id);
            if (userCoins < amount)
            {
                await ReplyFailureEmbed("You do not have enough Sora Coins for this transfer!");
                return;
            }

            // If the user is CACHED we just create him before hand. otherwise we do not and let it fail down below
            // if the user didnt use sora before
            var receiver = Context.Client.GetUser(receiverId);
            User receiverDb = null;
            if (receiver != null)
            {
                // Create the user
                var userMaybe = await _userRepo.GetOrCreateUser(receiverId).ConfigureAwait(false);
                if (await FailedToGetUser(userMaybe)) return;
                receiverDb = userMaybe.Value;
            }
            else
            {
                // ONLY get user.
                receiverDb = await _userRepo.GetUser(receiverId).ConfigureAwait(false);
            }

            // Then we try and get the receiver.
            // We only allow sending to users that have a SORA DB account
            // This way we can make sure the user exists on Discord without making a Rest client call. Speeding things up
            // If we can find the client CACHED though we create it up above so this wont fail. 
            if (receiverDb == null)
            {
                await ReplyFailureEmbedExtended(
                    "Could not find the specified user",
                    "Sora will only transfer money to users that are already in the Database. For that to happen " +
                    "the user has to use sora with a command like `daily` at least once before he can receive money.");
                return;
            }

            // We got the receiver Db and the user has enough money to make this transfer. Do it
            if (await FailedTryTransaction(
                await _coinRepo.TryMakeTransfer(Context.User.Id, receiverId, (uint) amount).ConfigureAwait(false),
                "The transaction failed. Either your balance changed or an account has been deleted. Please try again.")
            )
            {
                return;
            }

            bool notified = false;
            if (receiver != null)
            {
                try
                {
                    await (await receiver.GetOrCreateDMChannelAsync())
                        .SendMessageAsync("",
                            embed: SimpleEmbed(Purple,
                                    $"💰 You've received {amount.ToString()} SC from {Formatter.UsernameDiscrim(Context.User)}!")
                                .Build());
                    notified = true;
                }
                catch (Exception)
                {
                    notified = false;
                }
            }

            await ReplySuccessEmbed(
                $"You have successfully transferred {amount.ToString()} SC to {Formatter.UsernameDiscrim(receiver)}." +
                $"{(notified ? " They have been notified via DM." : "")}");
        }
    }
}