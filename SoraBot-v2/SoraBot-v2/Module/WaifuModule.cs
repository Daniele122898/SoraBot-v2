using System.Threading.Tasks;
using Discord.Commands;
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
        public async Task ShowMyWaifus()
        {
            await ReplyAsync($"Check out your Waifus here: http://sorabot.pw/user/{Context.User.Id}/waifus °˖✧◝(⁰▿⁰)◜✧˖°");
        }
    }
}