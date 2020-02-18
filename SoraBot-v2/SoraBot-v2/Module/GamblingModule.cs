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
        private DiscordRestClient _discordRestClient;

        public GamblingModule(CoinService coinService, DiscordRestClient restClient)
        {
            _coinService = coinService;
            _discordRestClient = restClient;
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
                    int score = GetResult(side);
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

        public int GetResult(string side)
        {
            int score = 0;
            var random = new Random();
            bool result = random.Next(100) % 2 == 0;
            switch (result)
            {
                case true:
                    if (string.Equals(side, "heads", StringComparison.CurrentCultureIgnoreCase))
                    {
                        score = 1;
                    }
                    break;

                case false:
                    if (string.Equals(side, "tails", StringComparison.CurrentCultureIgnoreCase))
                    {
                        score = 1;
                    }
                    break;
            }
            return score;
        }
    }
}