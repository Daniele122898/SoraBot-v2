using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using SoraBot.Data.Utils;

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
    }
}