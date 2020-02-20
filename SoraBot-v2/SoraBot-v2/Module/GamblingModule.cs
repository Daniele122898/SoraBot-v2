using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.Rest;
using SoraBot_v2.Data;
using SoraBot_v2.Services;

namespace SoraBot_v2.Module
{
    [Name("Gambling")]
    public class GamblingModule : ModuleBase<SocketCommandContext>
    {
        private CoinService _coinService;

        public GamblingModule(CoinService coinService)
        {
            _coinService = coinService;
        }

        [Command("coinflip", RunMode = RunMode.Async), Alias("cf"),
         Summary("Flips a coin!")]
        public async Task FlipCoin(int bet, [Remainder]string side)
        {
            using (var soraContext = new SoraContext())
            {
                var userDb = Utility.GetOrCreateUser(Context.User.Id, soraContext);

                if (bet > _coinService.GetAmount(Context.User.Id))
                {
                    await ReplyAsync($"You only have {userDb.Money} Coins.");
                    return;
                }

                if (string.Equals(side, "heads", StringComparison.CurrentCultureIgnoreCase)
                    || string.Equals(side, "tails", StringComparison.CurrentCultureIgnoreCase))
                {
                    bool score = GetResult(side);
                    if (score.Equals(true))
                    {
                        //won
                        await Context.Channel.SendMessageAsync("You won!");
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
                else
                {
                    await Context.Channel.SendMessageAsync($"{side} is not a valid option.");
                }
            }
        }

        public bool GetResult(string side)
        {
            Random random = new Random();
            return string.Equals(
                side,
                random.Next(100) % 2 == 0 ? "heads" : "tails",
                StringComparison.InvariantCultureIgnoreCase
            );
        }
    }
}