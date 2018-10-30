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

        [Command("unbox", RunMode = RunMode.Async), Alias("waifu"), Summary("Unbox Waifus")]
        public async Task UnboxWaifus()
        {
            await _waifuService.UnboxWaifu(Context);
        }

        [Command("special", RunMode = RunMode.Async), Alias("halloween"), Summary("Open Halloween Waifuboxes")]
        public async Task SpecialWaifus()
        {
            await _waifuService.UnboxSpecialWaifu(Context);
        }

        [Command("mywaifus"), Alias("waifus"), Summary("Shows all the waifus you or the specified user owns")]
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

        [Command("selldupes"), Alias("dupes", "quickselldupes"), Summary("Sells all dupes that you have.")]
        public async Task SellDupes()
        {
            await _waifuService.SellDupes(Context);
        }
        
        [Command("sell"), Alias("quicksell"), Summary("Quick sell waifus for some fast Sora Coins")]
        public async Task QuickSell(string name, int amount = 1)
        {
            // if amount is omitted it will default to one
            if (amount < 1)
            {
                await ReplyAsync("", embed: Utility.ResultFeedback(
                        Utility.RedFailiureEmbed,
                        Utility.SuccessLevelEmoji[2],
                        "Amount must be bigger than 0!")
                    .Build());
                return;
            }
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
                    ).Build());
                    return;
                }

                waifuId = waifu.Id;
            }
            await _waifuService.QuickSellWaifus(Context, waifuId, amount);
        }

        [Command("sell"), Alias("quicksell"), Summary("Quick sell waifus for some fast Sora Coins")]
        public async Task QuickSell(int waifuId, int amount = 1)
        {
            if (amount < 1)
            {
                await ReplyAsync("", embed: Utility.ResultFeedback(
                        Utility.RedFailiureEmbed,
                        Utility.SuccessLevelEmoji[2],
                        "Amount must be bigger than 0!")
                    .Build());
                return;
            }
            await _waifuService.QuickSellWaifus(Context, waifuId, amount);
        }
        
        
        [Command("setfavorite"), Alias("favorite", "bestwaifu", "fav", "favwaifu"), Summary("Sets your favorite waifu")]
        public async Task SetFavWaifu([Remainder] string name)
        {
            int waifuId = 0;
            using (var soraContext = new SoraContext())
            {
                name = name.Replace("\"", "");
                var waifu = soraContext.Waifus.FirstOrDefault(x =>
                    x.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase));
                if (waifu == null)
                {
                    await ReplyAsync("", embed: Utility.ResultFeedback(
                        Utility.RedFailiureEmbed,
                        Utility.SuccessLevelEmoji[2],
                        "That waifu doesn't exist."
                    ).Build());
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

        [Command("removefav"), Alias("unfavorite", "nowaifu"), Summary("Removes your favorite waifu")]
        public async Task RemoveFavWaifu()
        {
            using (var soraContext = new SoraContext())
            {
                var userdb = Utility.OnlyGetUser(Context.User.Id, soraContext);
                if (userdb == null)
                {
                    await ReplyAsync("", embed: Utility.ResultFeedback(
                        Utility.RedFailiureEmbed,
                        Utility.SuccessLevelEmoji[2],
                        "You don't have a favorite waifu..."
                    ).Build());
                    return;
                }

                userdb.FavoriteWaifu = -1;
                await soraContext.SaveChangesAsync();
                await ReplyAsync("", embed: Utility.ResultFeedback(
                    Utility.GreenSuccessEmbed,
                    Utility.SuccessLevelEmoji[0],
                    "Successfully removed favorite waifu."
                ).Build());
            }
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
                    ).Build());
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
                    ).Build());
                    return;
                }

                offerId = offerW.Id;
            }
            await _waifuService.MakeTradeOffer(Context, user, wantId, offerId);
        }
    }
}