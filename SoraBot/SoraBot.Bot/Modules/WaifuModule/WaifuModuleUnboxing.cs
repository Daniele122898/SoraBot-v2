using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Microsoft.Extensions.Options;
using SoraBot.Common.Extensions.Modules;
using SoraBot.Common.Utils;
using SoraBot.Data.Configurations;
using SoraBot.Data.Models.SoraDb;
using SoraBot.Data.Repositories.Interfaces;
using SoraBot.Services.Waifu;

namespace SoraBot.Bot.Modules.WaifuModule
{
    [Name("Waifus")]
    [Summary("Open Waifu Boxes, sell or trade them and collect them all!")]
    public partial class WaifuModule : SoraSocketCommandModule
    {
        private const int _WAIFU_BOX_COST = 500;
        private const int _WAIFU_BOX_SPECIAL_COST = 750;
        private const int _WAIFU_AMOUNT_IN_BOX = 3;
        
        private readonly IWaifuService _waifuService;
        private readonly ICoinRepository _coinRepo;
        private readonly InteractiveService _interactiveService;
        private readonly SoraBotConfig _config;

        public WaifuModule(
            IWaifuService waifuService, 
            IOptions<SoraBotConfig> config, 
            ICoinRepository coinRepo,
            InteractiveService interactiveService)
        {
            _config = config?.Value ?? throw new ArgumentNullException(nameof(config));
            _waifuService = waifuService;
            _coinRepo = coinRepo;
            _interactiveService = interactiveService;
        }

        [Command("special")]
        [Summary("Unboxes a Special waifu box. These are more expensive with 750 SC but have " +
                 "limited time Waifus")]
        public async Task OpenSpecialWaifuBox()
        {
            if (!_config.SpecialWaifuActive)
            {
                await ReplyFailureEmbed("There are no special waifus active right now :/");
                return;
            }
            // Check user cash
            var sc = _coinRepo.GetCoins(Context.User.Id);
            if (sc < _WAIFU_BOX_SPECIAL_COST)
            {
                await ReplyFailureEmbed($"You don't have enough Sora Coins! You need {_WAIFU_BOX_SPECIAL_COST.ToString()} SC.");
                return;
            }
        
            var special = await _waifuService.GetRandomSpecialWaifu(Context.User.Id, _config.SpecialWaifuType).ConfigureAwait(false);
            if (special == null)
            {
                await ReplyFailureEmbed("You already have all the special waifus. Pls try again another time.");
                return;
            }
            List<Waifu> waifusUnboxed = new List<Waifu>();
            waifusUnboxed.Add(special);
            int additional = _WAIFU_AMOUNT_IN_BOX - 1;
            for (int i = 0; i < additional; i++)
            {
                waifusUnboxed.Add(await _waifuService.GetRandomWaifu().ConfigureAwait(false));
            }
            if (waifusUnboxed.Count != _WAIFU_AMOUNT_IN_BOX)
            {
                await ReplyFailureEmbed("There don't seem to be Waifus to unbox at the moment. Sorry :/");
                return;
            }
            
            // Now lets try to give everything to the user before we continue doing anything else
            if (!await _waifuService.TryGiveWaifusToUser(Context.User.Id, waifusUnboxed, _WAIFU_BOX_SPECIAL_COST).ConfigureAwait(false))
            {
                await ReplyFailureEmbed("Failed to give Waifus :( Please try again");
                return;
            }
            // We gave him the waifus. Now we just have to tell him :)
            waifusUnboxed.Sort((x, y) =>  -x.Rarity.CompareTo(y.Rarity));

            var eb = new EmbedBuilder()
            {
                Title = "Congrats! You've got some nice waifus",
                Description = $"You opened a {WaifuFormatter.GetRarityString(_config.SpecialWaifuType)} WaifuBox for {_WAIFU_BOX_SPECIAL_COST.ToString()} SC.",
                Footer = RequestedByFooter(Context.User),
                Color = Purple,
                ImageUrl = waifusUnboxed[0].ImageUrl
            };

            foreach (var waifu in waifusUnboxed)
            {
                eb.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = waifu.Name;
                    x.Value = $"Rarity: {WaifuFormatter.GetRarityString(waifu.Rarity)}\n" +
                              $"[Image Url]({waifu.ImageUrl})\n" +
                              $"*ID: {waifu.Id}*";
                });
            }
            
            await ReplyAsync("", embed: eb.Build());
        }

        [Command("waifu"), Alias("unbox")]
        [Summary("Open a Waifu Box for 500 Sora Coins")]
        public async Task OpenWaifuBox()
        {
            // Check the user cash
            var sc = _coinRepo.GetCoins(Context.User.Id);
            if (sc < _WAIFU_BOX_COST)
            {
                await ReplyFailureEmbed($"You don't have enough Sora Coins! You need {_WAIFU_BOX_COST.ToString()} SC.");
                return;
            }
            
            // Get the waifus
            List<Waifu> waifusUnboxed = new List<Waifu>();
            for (int i = 0; i < _WAIFU_AMOUNT_IN_BOX; ++i)
            {
                var wToAdd = await _waifuService.GetRandomWaifu().ConfigureAwait(false);
                if (wToAdd ==  null) break;
                // Check if URL is valid bcs it seems some are kinda broken ;_;
                // This is an extreme edge case but it happened once and that is enough as it should never happen
                if (!Helper.UrlValidUri(wToAdd.ImageUrl))
                {
                    await _waifuService.RemoveWaifu(wToAdd.Id).ConfigureAwait(false);
                    --i;
                    continue;
                }
                waifusUnboxed.Add(wToAdd);
            }
            if (waifusUnboxed.Count != _WAIFU_AMOUNT_IN_BOX)
            {
                await ReplyFailureEmbed("There don't seem to be Waifus to unbox at the moment. Sorry :/");
                return;
            }
            
            // Now lets try to give everything to the user before we continue doing anything else
            if (!await _waifuService.TryGiveWaifusToUser(Context.User.Id, waifusUnboxed, _WAIFU_BOX_COST).ConfigureAwait(false))
            {
                await ReplyFailureEmbed("Failed to give Waifus :( Please try again");
                return;
            }
            // We gave him the waifus. Now we just have to tell him :)
            waifusUnboxed.Sort((x, y) =>  -x.Rarity.CompareTo(y.Rarity));

            string waifuImageUrl = waifusUnboxed.FirstOrDefault(x => Uri.IsWellFormedUriString(x.ImageUrl, UriKind.Absolute))?.ImageUrl;

            var eb = new EmbedBuilder()
            {
                Title = "Congrats! You've got some nice waifus",
                Description = $"You opened a regular WaifuBox for {_WAIFU_BOX_COST.ToString()} SC." +
                              $@"{(_config.SpecialWaifuActive ?
                                  $"\nThere are currently {WaifuFormatter.GetRarityString(_config.SpecialWaifuType)} special Waifus for a limited time only. " +
                                  $"You can open them with the `special` command" : $"")}",
                Footer = RequestedByFooter(Context.User),
                Color = Purple,
                ImageUrl = waifuImageUrl
            };

            foreach (var waifu in waifusUnboxed)
            {
                eb.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = waifu.Name;
                    x.Value = $"Rarity: {WaifuFormatter.GetRarityString(waifu.Rarity)}\n" +
                              $"[Image Url]({waifu.ImageUrl})\n" +
                              $"*ID: {waifu.Id}*";
                });
            }
            
            await ReplyAsync("", embed: eb.Build());
        }

        [Command("request"), Alias("request waifu", "requestwaifu"),
         Summary("Posts the link where you can request waifus")]
        public async Task RequestWaifuLink()
            => await ReplyAsync("You can request waifus here:\n https://request.sorabot.pw/");
    }
}