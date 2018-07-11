using System;
using System.Threading.Tasks;
using Discord.Commands;
using Humanizer;
using Humanizer.Localisation;
using SoraBot_v2.Data;
using SoraBot_v2.Services;

namespace SoraBot_v2.Module
{
    public class CoinModule : ModuleBase<SocketCommandContext>
    {
        private CoinService _coinService;

        public CoinModule(CoinService coinService)
        {
            _coinService = coinService;
        }
        
        // Gain coins
        [Command("daily"), Alias("earn", "dailies"), Summary("Gives you a daily reward of Sora Coins")]
        public async Task GetDaily()
        {
            await _coinService.DoDaily(Context);
        }
        
        // TODO give coins
        
        // TODO check coins
        [Command("coins"), Alias("sc"), Summary("Check how many Sora coins you have")]
        public async Task GetCoins()
        {
            int amount = _coinService.GetAmount(Context.User.Id);
            await ReplyAsync("", embed: Utility.ResultFeedback(
                Utility.BlueInfoEmbed,
                Utility.SuccessLevelEmoji[4],
                $"💰 You have {amount} Sora Coins."
                ));
        }
        
    }
}