using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.Rest;
using SoraBot_v2.Data;
using SoraBot_v2.Extensions;
using SoraBot_v2.Services;

namespace SoraBot_v2.Module
{
    [Name("Gambling")]
    public class GamblingModule : ModuleBase<SocketCommandContext>
    {
        private readonly CoinService _coinService;

        public GamblingModule(CoinService coinService)
        {
            _coinService = coinService;
        }

        [Command("coinflip", RunMode = RunMode.Async), Alias("cf"), Summary("Flips a coin! Either win double your bet or loose it all")]
        public async Task FlipCoin(int bet, [Remainder] string side)
        {
            if (bet <= 0)
            {
                await this.ReplySoraEmbedResponse(Utility.RedFailiureEmbed, Utility.FailiureEmoji,
                    "Please bet a positive amount greater than 0");
                return;
            }
            
            using var soraContext = new SoraContext();
            var userDb = Utility.GetOrCreateUser(Context.User.Id, soraContext);

            if (bet > _coinService.GetAmount(Context.User.Id))
            {
                await this.ReplySoraEmbedResponse(Utility.RedFailiureEmbed, Utility.FailiureEmoji, 
                    $"You don't have enough money for this bet! You currently have {userDb.Money} SC");
                return;
            }

            side = side.Trim(); // just in case D.net doesn't do that
            if (!string.Equals(side, "heads", StringComparison.InvariantCultureIgnoreCase)
                && !string.Equals(side, "tails", StringComparison.InvariantCultureIgnoreCase))
            {
                await this.ReplySoraEmbedResponse(Utility.RedFailiureEmbed, Utility.FailiureEmoji,
                    $"The side you chose is invalid. Please use `heads` or `tails`");
                return;
            }
                
            Random random = new Random();

            bool userWon = side.Equals(random.Next(100) % 2 == 0 ? "heads" : "tails",
                StringComparison.InvariantCultureIgnoreCase);
            int winnings = userWon ? bet : -bet;
            if (!await _coinService.AddCoinAmount(userDb, winnings))
            {
                await this.ReplySoraEmbedResponse(Utility.RedFailiureEmbed, Utility.FailiureEmoji,
                    "Failed to acquire your Sora coin information. Please try again.");
                return;
            }
                
            await soraContext.SaveChangesAsync();

            if (userWon)
            {
                await this.ReplySoraEmbedResponse(Utility.GreenSuccessEmbed, Utility.PartyEmoji,
                    $"Congratulations! You won {winnings} SC!");
            }
            else
            {
                await this.ReplySoraEmbedResponse(Utility.YellowWarningEmbed, Utility.NoEmoji,
                    $"You lost :(");
            }
        }
    }
}