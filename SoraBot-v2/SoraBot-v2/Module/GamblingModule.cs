using System;
using System.Threading.Tasks;
using Discord.Commands;
using SoraBot_v2.Data;
using SoraBot_v2.Services;

namespace SoraBot_v2.Module
{
    [Name("Gambling")]
    public class GamblingModule : ModuleBase<SocketCommandContext>
    {
        [Command("coinflip", RunMode = RunMode.Async), Alias("cf"),
         Summary("Flips a coin!")]
        public async Task FlipCoin(int bet, [Remainder]string side)
        {
            var soraContext = new SoraContext();
            var restClient = new Discord.Rest.DiscordRestClient();
            var coinService = new CoinService(restClient);
            var gamblingService = new GamblingService();
            var userDb = Utility.GetOrCreateUser(Context.User.Id, soraContext);

            if (bet > coinService.GetAmount(Context.User.Id))
            {
                await ReplyAsync($"You only have {userDb.Money} Coins.");
                return;
            }

            if (string.Equals(side, "heads", StringComparison.CurrentCultureIgnoreCase)
                || string.Equals(side, "tails", StringComparison.CurrentCultureIgnoreCase))
            {
                int score = gamblingService.GetResult(side);
                if (score == 1)
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
}