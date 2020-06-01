using System;
using System.Threading.Tasks;
using Discord.Commands;
using SoraBot.Common.Extensions.Modules;
using SoraBot.Data.Repositories.Interfaces;
using SoraBot.Services.Utils;

namespace SoraBot.Bot.Modules
{
    [Name("Gambling")]
    [Summary("All commands around gambling and loosing all your Sora Coins ;)")]
    public class GamblingModule : SoraSocketCommandModule
    {
        private readonly ICoinRepository _coinRepository;
        private readonly RandomNumberService _rand;

        public GamblingModule(ICoinRepository coinRepository, RandomNumberService rand)
        {
            _coinRepository = coinRepository;
            _rand = rand;
        }

        [Command("dice"), Alias("roll")]
        [Summary("Roll a dice, bet on which number it lands. If you win you'll get 6x the amount you bet.")]
        public async Task RollDice(int bet, int side)
        {
            if (side < 1 || side > 6)
            {
                await ReplyFailureEmbed("Please choose a side between 1-6");
                return;
            }

            if (bet <= 0)
            {
                await ReplyFailureEmbed("Please specify a bet larger than 0.");
                return;
            }
            
            var availableCoins = _coinRepository.GetCoins(Context.User.Id);
            if (bet > availableCoins)
            {
                await ReplyFailureEmbed(
                    $"Please specify a bet smaller or equal to your total Sora coin amount ({availableCoins.ToString()} SC).");
                return;
            }

            int roll = _rand.GetRandomNext(1, 7);
            bool won = roll == side;
            if (won)
                await _coinRepository.GiveAmount( Context.User.Id, (uint)(bet * 5)); // *5 since we never take away the initial bet
            else if (!await _coinRepository.TryTakeAmount(Context.User.Id, (uint) bet))
            {
                await ReplyFailureEmbed("You didn't have enough Sora coins for the transfer. Try again");
                return;
            }

            if (won)
                await ReplyMoneyEmbed($"Congratulations! You won {(bet * 6).ToString()} Sora Coins!");
            else
                await ReplyMoneyLostEmbed($"You lost :( The dice was {roll.ToString()}. Better luck next time!");
        }

        [Command("coinflip"), Alias("cf", "flip")]
        [Summary("Flip a coin and bet on a side! If you win you'll get double your bet")]
        public async Task FlipCoin(int bet, string side)
        {
            if (bet <= 0)
            {
                await ReplyFailureEmbed("Please bet an amount greater than 0!");
                return;
            }
            side = side.Trim();

            if (!side.Equals("heads", StringComparison.OrdinalIgnoreCase) &&
                !side.Equals("tails", StringComparison.OrdinalIgnoreCase))
            {
                await ReplyFailureEmbed("Please choose between `heads` or `tails` only.");
                return;
            }
            
            var availableCoins = _coinRepository.GetCoins(Context.User.Id);
            if (bet > availableCoins)
            {
                await ReplyFailureEmbed(
                    $"Please specify a bet smaller or equal to your total Sora coin amount ({availableCoins.ToString()} SC).");
                return;
            }

            // Instead of choosing a side we just give the user a 50% chance to win. 
            // Since mathematically speaking its the same. We then decide on the side
            // based on the fact if he won or lost.
            bool win = _rand.GetRandomNext(100) % 2 == 0;
            if (win)
                await _coinRepository.GiveAmount(Context.User.Id, (uint) (bet));
            else if (!await _coinRepository.TryTakeAmount(Context.User.Id, (uint) bet))
            {
                await ReplyFailureEmbed("You didn't have enough Sora coins for the transfer. Try again");
                return;
            }
            
            if (win)
                await ReplyMoneyEmbed($"Congratulations! You won {(bet * 2).ToString()} Sora Coins!");
            else
                await ReplyMoneyLostEmbed($"You lost :( Better luck next time!");
        }
    }
}