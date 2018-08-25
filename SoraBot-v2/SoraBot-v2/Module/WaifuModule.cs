using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
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
            await ReplyAsync($"Check out all Waifus here: http://sorabot.pw/allwaifus °˖✧◝(⁰▿⁰)◜✧˖°");
        }

        [Command("sell"), Alias("quicksell"), Summary("Quick sell waifus for some fast Sora Coins")]
        public async Task QuickSell(int waifuId, int amount)
        {
            await _waifuService.QuickSellWaifus(Context, waifuId, amount);
        }
    }
}