﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Options;
using SoraBot.Common.Extensions.Modules;
using SoraBot.Data.Configurations;
using SoraBot.Data.Models.SoraDb;
using SoraBot.Data.Repositories.Interfaces;
using SoraBot.Services.Waifu;

namespace SoraBot.Bot.Modules
{
    public class WaifuModule : SoraSocketCommandModule
    {
        public const int WAIFU_BOX_COST = 500;
        public const int WAIFU_AMOUNT_IN_BOX = 3;
        
        private readonly IWaifuService _waifuService;
        private readonly ICoinRepository _coinRepo;
        private SoraBotConfig _config;

        public WaifuModule(
            IWaifuService waifuService, 
            IOptions<SoraBotConfig> config, 
            ICoinRepository coinRepo)
        {
            _config = config?.Value ?? throw new ArgumentNullException(nameof(config));
            _waifuService = waifuService;
            _coinRepo = coinRepo;
        }

        [Command("waifu")]
        public async Task OpenWaifuBox()
        {
            // Check the user cash
            var sc = _coinRepo.GetCoins(Context.User.Id);
            if (sc < WAIFU_BOX_COST)
            {
                await ReplyFailureEmbed($"You don't have enough Sora Coins! You need {WAIFU_BOX_COST} SC.");
                return;
            }
            
            // Get the waifus
            List<Waifu> waifusUnboxed = new List<Waifu>();
            for (int i = 0; i < WAIFU_AMOUNT_IN_BOX; i++)
            {
                waifusUnboxed.Add(await _waifuService.GetRandomWaifu().ConfigureAwait(false));
            }
            if (waifusUnboxed.Count != WAIFU_AMOUNT_IN_BOX)
            {
                await ReplyFailureEmbed("There don't seem to be Waifus to unbox at the moment. Sorry :/");
                return;
            }
            
            // Now lets try to give everything to the user before we continue doing anything else
            if (!await _waifuService.TryGiveWaifusToUser(Context.User.Id, waifusUnboxed, WAIFU_BOX_COST).ConfigureAwait(false))
            {
                await ReplyFailureEmbed("Failed to give Waifus :( Please try again");
                return;
            }
            // We gave him the waifus. Now we just have to tell him :)
            waifusUnboxed.Sort((x, y) =>  -x.Rarity.CompareTo(y.Rarity));

            var eb = new EmbedBuilder()
            {
                Title = "Congrats! You've got some nice waifus",
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
    }
}