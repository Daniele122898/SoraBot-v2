using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using SoraBot_v2.Data;
using SoraBot_v2.Services;

namespace SoraBot_v2.Module
{
    public class WaifuModule : ModuleBase<SocketCommandContext>
    {
        private WaifuService _waifuService;

        public WaifuModule(WaifuService waifuService)
        {
            _waifuService = waifuService;
        }
        
        [Command("addwaifu")]
        [RequireOwner]
        public async Task AddWaifu(string name, string image, int rarity)
        {
            await _waifuService.AddWaifu(Context, name, image, rarity);
        }

        [Command("unbox"), Alias("waifu"), Summary("Unbox Waifus")]
        public async Task UnboxWaifus()
        {
            await _waifuService.UnboxWaifu(Context);
        }

        [Command("mywaifus"), Alias("waifus"), Summary("Shows all the waifus you own")]
        public async Task ShowMyWaifus(SocketUser userT = null)
        {
            var user = userT ?? Context.User;
            await ReplyAsync($"Check out **{user.Username}'s Waifus** here: http://sorabot.pw/user/{user.Id}/waifus °˖✧◝(⁰▿⁰)◜✧˖°");
        }
        
        [Command("allwaifus"), Alias("waifulist", "wlist"), Summary("Shows all the waifus that exist")]
        public async Task ShowAllWaifus()
        {
            await ReplyAsync($"Check out **all Waifus** here: http://sorabot.pw/allwaifus °˖✧◝(⁰▿⁰)◜✧˖°");
        }
        
        [Command("sell"), Alias("quicksell"), Summary("Quick sell waifus for some fast Sora Coins")]
        public async Task QuickSell(string name, int amount)
        {
            int waifuId = 0;
            using (var soraContext = new SoraContext())
            {
                var waifu = soraContext.Waifus.FirstOrDefault(x =>
                    x.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase));
                if (waifu == null)
                {
                    await ReplyAsync("", embed: Utility.ResultFeedback(
                        Utility.RedFailiureEmbed,
                        Utility.SuccessLevelEmoji[2],
                        "That waifu doesn't exist. Make sure to wrap the name in \"\" if it consists of more than 1 word!"
                    ));
                    return;
                }

                waifuId = waifu.Id;
            }
            await _waifuService.QuickSellWaifus(Context, waifuId, amount);
        }

        [Command("sell"), Alias("quicksell"), Summary("Quick sell waifus for some fast Sora Coins")]
        public async Task QuickSell(int waifuId, int amount)
        {
            await _waifuService.QuickSellWaifus(Context, waifuId, amount);
        }
        
        [Command("setfavorite"), Alias("favorite", "bestwaifu", "fav", "favwaifu"), Summary("Sets your favorite waifu")]
        public async Task SetFavWaifu([Remainder] string name)
        {
            int waifuId = 0;
            using (var soraContext = new SoraContext())
            {
                var waifu = soraContext.Waifus.FirstOrDefault(x =>
                    x.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase));
                if (waifu == null)
                {
                    await ReplyAsync("", embed: Utility.ResultFeedback(
                        Utility.RedFailiureEmbed,
                        Utility.SuccessLevelEmoji[2],
                        "That waifu doesn't exist."
                    ));
                    return;
                }

                waifuId = waifu.Id;
            }
            await _waifuService.SetFavoriteWaifu(Context, waifuId);
        }

        [Command("setfavorite"), Alias("favorite", "bestwaifu", "fav", "favwaifu"), Summary("Sets your favorite waifu")]
        public async Task SetFavWaifu(int waifuId)
        {
            await _waifuService.SetFavoriteWaifu(Context, waifuId);
        }

        [Command("trade", RunMode = RunMode.Async), Alias("tradewaifu", "waifutrade"), Summary("Trade Waifus")]
        public async Task TradeWaifu(SocketGuildUser user, int wantId, int offerId)
        {
            await _waifuService.MakeTradeOffer(Context, user, wantId, offerId);
        }
        
        [Command("trade", RunMode = RunMode.Async), Alias("tradewaifu", "waifutrade"), Summary("Trade Waifus")]
        public async Task TradeWaifu(SocketGuildUser user, string want, string offer)
        {
            int wantId = 0;
            int offerId = 0;
            using (var soraContext = new SoraContext())
            {
                var wantW = soraContext.Waifus.FirstOrDefault(x =>
                    x.Name.Equals(want.Trim(), StringComparison.OrdinalIgnoreCase));
                if (wantW == null)
                {
                    await ReplyAsync("", embed: Utility.ResultFeedback(
                        Utility.RedFailiureEmbed,
                        Utility.SuccessLevelEmoji[2],
                        $"`{want}` doesn't exist. Make sure to wrap the name in \"\" if it consists of more than 1 word!"
                    ));
                    return;
                }
                wantId = wantW.Id;

                var offerW = soraContext.Waifus.FirstOrDefault(x =>
                    x.Name.Equals(offer.Trim(), StringComparison.OrdinalIgnoreCase));
                if (offerW == null)
                {
                    await ReplyAsync("", embed: Utility.ResultFeedback(
                        Utility.RedFailiureEmbed,
                        Utility.SuccessLevelEmoji[2],
                        $"`{offer}` doesn't exist. Make sure to wrap the name in \"\" if it consists of more than 1 word!"
                    ));
                    return;
                }

                offerId = offerW.Id;
            }
            await _waifuService.MakeTradeOffer(Context, user, wantId, offerId);
        }
    }
}