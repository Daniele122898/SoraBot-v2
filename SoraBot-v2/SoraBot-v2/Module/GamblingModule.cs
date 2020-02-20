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

        [Command("coinflip"), Alias("cf"),
         Summary("Flips a coin!")]
        public async Task FlipCoin(int bet, [Remainder]string side)
        {
            using (var soraContext = new SoraContext())
            {
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

                bool score = side.Equals(random.Next(100) % 2 == 0 ? "heads" : "tails",
                    StringComparison.InvariantCultureIgnoreCase);
                var lck = _coinService.GetOrCreateLock(Context.User.Id);
                if (score)
                {
                    //won
                    int winnings = bet * 2;
                    await this.ReplySoraEmbedResponse(Utility.GreenSuccessEmbed, Utility.PartyEmoji,
                        $"Congratulations! You won {winnings}!");
                    userDb.Money += bet * 2;
                    await soraContext.SaveChangesAsync();
                }
                else
                {
                    //lost
                    await Context.Channel.SendMessageAsync("You lost!");
                    userDb.Money -= bet;
                    await soraContext.SaveChangesAsync();
                }
            }
        }
    }
}