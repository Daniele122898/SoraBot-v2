using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using SoraBot.Bot.Extensions.Interactive;
using SoraBot.Bot.Models;
using SoraBot.Common.Utils;
using SoraBot.Data.Models.SoraDb;

namespace SoraBot.Bot.Modules.WaifuModule
{
    public partial class WaifuModule
    {
        [Command("trade"), Alias("tradewaifu", "waifutrade")]
        [Summary("Trade a Waifu for a Waifu from someone else. This will create a request " +
                 "that they can accept or decline.")]
        public async Task TradeWaifu(
            [Summary("@Mention the user you want to trade with")]
            DiscordUser tradeUser,
            [Summary("The Name of the Waifu you want from them. Wrap the name in \"\" otherwise it wont work.")]
            string wantName,
            [Summary("The Name of the Waifu you offer to give them. Wrap the name in \"\" otherwise it wont work.")]
            string offerName)
        {
            var wantWaifu = await _waifuService.GetWaifuByName(wantName.Trim()).ConfigureAwait(false);
            if (wantWaifu == null)
            {
                await ReplyFailureEmbed("The Waifu you want doesn't exist. Make sure you wrap the names in \"\"!");
                return;
            }
            
            var offerwWaifu = await _waifuService.GetWaifuByName(offerName.Trim()).ConfigureAwait(false);
            if (offerwWaifu == null)
            {
                await ReplyFailureEmbed("The Waifu you offer doesn't exist. Make sure you wrap the names in \"\"!");
                return;
            }

            await this.WaifuTradeComp(tradeUser.User, wantWaifu.Id, offerwWaifu.Id).ConfigureAwait(false);
        }

        [Command("trade"), Alias("tradewaifu", "waifutrade")]
        [Summary("Trade a Waifu for a Waifu from someone else. This will create a request " +
                 "that they can accept or decline.")]
        public async Task TradeWaifu(
            [Summary("@Mention the user you want to trade with")]
            DiscordUser tradeUser,
            [Summary("The ID of the Waifu you want from them")]
            int wantId,
            [Summary("The ID of the Waifu you offer to give them")]
            int offerId)
        {
            await this.WaifuTradeComp(tradeUser.User, wantId, offerId).ConfigureAwait(false);
        }

        private async Task WaifuTradeComp(IUser tradeUser, int wantId, int offerId)
        {
            // First we gotta make sure that the users have the respective waifus
            var wantUserWaifu = await _waifuService.GetUserWaifu(tradeUser.Id, wantId).ConfigureAwait(false);
            if (wantUserWaifu == null)
            {
                await ReplyFailureEmbed("The user does not have the Waifu you want.");
                return;
            }
            var offerUserWaifu = await _waifuService.GetUserWaifu(Context.User.Id, offerId).ConfigureAwait(false);
            if (offerUserWaifu == null)
            {
                await ReplyFailureEmbed("You do not have the Waifu that you offer!");
                return;
            }
            
            // They both have both. So we can actually ask for the trade. 
            // No further preparations or queries until the user actually accepts the trade
            var wantWaifu = await _waifuService.GetWaifuById(wantId).ConfigureAwait(false);
            var offerWaifu = await _waifuService.GetWaifuById(offerId).ConfigureAwait(false);
            if (wantWaifu == null || offerWaifu == null)
            {
                await ReplyFailureEmbed(
                    "Could not find one of the Waifus. They might have been removed from the DB...");
                return;
            }
            var eb = new EmbedBuilder()
            {
                Title = "Waifu Trade Request",
                Footer = RequestedByFooter(Context.User),
                Description = $"{Formatter.UsernameDiscrim(Context.User)} wishes to trade with you.",
                Color = Purple,
                ImageUrl = offerWaifu.ImageUrl,
            };
            eb.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "User offers";
                x.Value =
                    $"{offerWaifu.Name}\n{WaifuFormatter.GetRarityString(offerWaifu.Rarity)}\n_ID: {offerWaifu.Id}_";
            });
            eb.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "User wants";
                x.Value =
                    $"{wantWaifu.Name}\n{WaifuFormatter.GetRarityString(wantWaifu.Rarity)}\n_ID: {wantWaifu.Id}_";
            });
            eb.AddField(x =>
            {
                x.IsInline = false;
                x.Name = "Accept?";
                x.Value = "You can accept this trade request by writing `y` or `yes` (nothing else). " +
                          "If you write anything else Sora will count that as declining the offer.";
            });

            await ReplyAsync("", embed: eb.Build());
            // Now wait for response.
            var criteria = InteractiveServiceExtensions.CreateEnsureFromUserInChannelCriteria(tradeUser.Id, Context.Channel.Id);
            var resp = await _interactiveService.NextMessageAsync(Context, criteria, TimeSpan.FromSeconds(45))
                .ConfigureAwait(false);
            if (resp == null)
            {
                await ReplyFailureEmbed($"{Formatter.UsernameDiscrim(tradeUser)} didn't answer in time >.<");
                return;
            }
            if (!InteractiveServiceExtensions.StringIsYOrYes(resp.Content))
            {
                await ReplyFailureEmbed($"{Formatter.UsernameDiscrim(tradeUser)} declined the trade offer.");
                return;
            }
            
            // User accepted offer.
            if (!await _waifuService.TryTradeWaifus(Context.User.Id, tradeUser.Id, offerId, wantId)
                .ConfigureAwait(false))
            {
                await ReplyFailureEmbed("Failed to make trade. Please try again");
                return;
            }

            await ReplySuccessEmbedExtended("Successfully traded Waifus!", $"{Formatter.UsernameDiscrim(Context.User)} got {wantWaifu.Name}\n" +
                                                                           $"{Formatter.UsernameDiscrim(tradeUser)} got {offerWaifu.Name}");
        }
        
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

            await QuickSellComp(waifu, amount);
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

            await QuickSellComp(waifu, amount);
        }

        private async Task QuickSellComp(Waifu waifu, int amount)
        {
            var res = await _waifuService.TrySellWaifu(Context.User.Id, waifu.Id, (uint) amount, waifu.Rarity).ConfigureAwait(false);
            if (res.HasError)
            {
                await ReplyFailureEmbed(res.Error.Message);
                return;
            }

            await ReplySuccessEmbed($"You successfully sold {amount.ToString()} Waifus for {res.Value.ToString()} SC");
        }
    }
}