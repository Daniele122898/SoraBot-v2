using System;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
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
        
        // give coins
        [Command("send"), Alias("transfer", "sctransfer", "sendcoins", "sendsc", "give"),
         Summary("Sends specified amount of sc to specified user")]
        public async Task SendCoins(int amount, ulong userId)
        {
            await _coinService.SendMoney(Context, amount, userId);
        }
        
        [Command("send"), Alias("transfer", "sctransfer", "sendcoins", "sendsc", "give"),
         Summary("Sends specified amount of sc to specified user")]
        public async Task SendCoins(int amount, SocketUser user)
        {
            await _coinService.SendMoney(Context, amount, user.Id);
        }
        
        // check coins
        [Command("coins"), Alias("soracoins"), Summary("Check how many Sora coins you have")]
        public async Task GetCoins()
        {
            int amount = _coinService.GetAmount(Context.User.Id);
            await ReplyAsync("", embed: Utility.ResultFeedback(
                Utility.BlueInfoEmbed,
                Utility.SuccessLevelEmoji[4],
                $"💰 You have {amount.ToString()} Sora Coins."
                ));
        }
        
    }
}