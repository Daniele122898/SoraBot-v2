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
    }
}