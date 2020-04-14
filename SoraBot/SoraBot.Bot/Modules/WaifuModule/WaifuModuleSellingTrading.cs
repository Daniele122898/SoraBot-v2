using System.Threading.Tasks;
using Discord.Commands;

namespace SoraBot.Bot.Modules.WaifuModule
{
    public partial class WaifuModule
    {
        [Command("selldupes"), Alias("dupes")]
        [Summary(
            "Sells all the dupes that you have. This will NOT sell Ultimate Waifus!")]
        public async Task SellWaifuDupes()
        {
            var resp = await _waifuService.SellDupes(Context.User.Id).ConfigureAwait(false);
            if (resp.HasError)
            {
                await ReplyFailureEmbed(resp.Error.Message);
                return;
            }
            var sold = resp.Value;
            await ReplySuccessEmbed($"You successfully sold {sold.waifusSold.ToString()} Waifus for {sold.coinAmount.ToString()} Sora Coins!");
        }

        [Command("sell")]
        [Alias("quicksell")]
        [Summary("Quick sells a Waifu for some quick Sora Coins.")]
        public async Task QuickSell(
            [Summary("How many of the Waifu you wish to sell")]
            int amount,
            [Summary("The EXACT name of the Waifu to sell"), Remainder]
            string waifuName)
        {
            waifuName = waifuName.Trim();
            var waifu = await _waifuService.GetWaifuByName(waifuName).ConfigureAwait(false);
            if (waifu == null)
            {
                await ReplyFailureEmbed("This Waifu doesn't exist. Make sure you spelled her name EXACTLY right!");
                return;
            }
        }
        
        [Command("sell")]
        [Alias("quicksell")]
        [Summary("Quick sells a Waifu for some quick Sora Coins.")]
        public async Task QuickSell(
            [Summary("How many of the Waifu you wish to sell")]
            int amount,
            [Summary("The ID of the Waifu to sell")]
            int waifuId)
        {
            var waifu = await _waifuService.GetWaifuById(waifuId).ConfigureAwait(false);
            if (waifu == null)
            {
                await ReplyFailureEmbed("This Waifu doesn't exist. Make sure you have the correct ID.");
                return;
            }
        }
    }
}