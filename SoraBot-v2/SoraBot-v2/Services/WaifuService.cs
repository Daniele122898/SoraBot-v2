using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using SoraBot_v2.Data;
using SoraBot_v2.Data.Entities;
using SoraBot_v2.Data.Entities.SubEntities;
using SoraBot_v2.Extensions;

namespace SoraBot_v2.Services
{
    
    /*
        Waifu trade
        ---------------
        
        Rarities
        ---------
        common				500 - 50
        uncommon			300 - 100
        rare				100 - 200
        epic				50 - 500
        ultimate Waifu		10 - 1000
        
        Waifu
        ---------
        id
        name
        imageurl
        rarity
        
        User Ownership
        -----------------
        id increment
        userid
        waifuId
        count
     */
    
    public class WaifuService
    {
        // TODO show waifus, sell, maybe trade, maybe fav
        private List<Waifu> _boxCache = new List<Waifu>();

        private int BOX_COST = 500;
        private byte BOX_CARD_AMOUNT = 3;
        private Random Rnd = new Random();
        
        public void Initialize()
        {
            // initial setup
            CreateRandomCache();
        }

        private void CreateRandomCache()
        {
            using (var soraContext = new SoraContext())
            {
                // get all waifus
                var waifus = soraContext.Waifus.ToArray();
                foreach (var waifu in waifus)
                {
                    // add each waifu * rarity amount to cache
                    int amount = GetRarityAmount(waifu.Rarity);
                    for (int i = 0; i < amount; i++)
                    {
                        _boxCache.Add(waifu);
                    }
                }
                // shuffle for some extra RNG
                _boxCache.Shuffle();
            }
        }

        private void GiveWaifuToId(ulong userId, int waifuId, User userdb)
        {
            var userWaifu = userdb.UserWaifus.FirstOrDefault(x => x.WaifuId == waifuId);
            // user does not have that waifu yet
            if (userWaifu == null)
            {
                userdb.UserWaifus.Add(new UserWaifu()
                {
                    Count = 1,
                    User = userdb,
                    UserForeignId = userId,
                    WaifuId = waifuId
                });
                return;
            }
            // otherwise increment count
            userWaifu.Count++;
        }

        public async Task AddWaifu(SocketCommandContext context, string name, string image, int rarity)
        {
            using (var soraContext = new SoraContext())
            {
                // check if waifu already exists
                if (soraContext.Waifus.Any(x=> x.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                {
                    await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                        Utility.RedFailiureEmbed,
                        Utility.SuccessLevelEmoji[2],
                        $"{name} already exists in the Database!"
                    ));
                    return;
                }
                
                var waifu = new Waifu()
                {
                    ImageUrl = image,
                    Name = name,
                    Rarity = GetRarityByInt(rarity),
                };
                soraContext.Waifus.Add(waifu);
                await soraContext.SaveChangesAsync();
                var withId =
                    soraContext.Waifus.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                AddWaifuToCache(withId);
                await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                    Utility.GreenSuccessEmbed,
                    Utility.SuccessLevelEmoji[0],
                    $"You added {name} with ID {waifu.Id}"
                ));
            }
        }
        
        public async Task UnboxWaifu(SocketCommandContext context)
        {
            using (var soraContext = new SoraContext())
            {
                var userdb = Utility.GetOrCreateUser(context.User.Id, soraContext);
                // check cash
                if (userdb.Money < BOX_COST)
                {
                    await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                        Utility.RedFailiureEmbed,
                        Utility.SuccessLevelEmoji[2],
                        $"You don't have enough Sora Coins! You need {BOX_COST} SC."
                    ));
                    return;
                }
                // remove money
                userdb.Money -= BOX_COST;
                // open box
                var waifus = new List<Waifu>();
                for (int i = 0; i < BOX_CARD_AMOUNT; i++)
                {
                    var waifu = GetRandomFromBox();
                    waifus.Add(waifu);
                    // add to user
                    GiveWaifuToId(context.User.Id, waifu.Id, userdb);
                }
                // save already if smth down there fails at least he got everything.
                await soraContext.SaveChangesAsync();
                // show what he got
                var ordered = waifus.OrderByDescending(x => x.Rarity).ToArray();
                var eb = new EmbedBuilder()
                {
                    Title = "Congrats! You've got some nice waifus",
                    Footer = Utility.RequestedBy(context.User),
                    Color = Utility.PurpleEmbed,
                    ImageUrl = ordered[0].ImageUrl
                };

                foreach (var waifu in ordered)
                {
                    eb.AddField(x =>
                    {
                        x.IsInline = true;
                        x.Name = waifu.Name;
                        x.Value = $"Rarity: {GetRarityString(waifu.Rarity)}\n" +
                                  $"[Image Url]({waifu.ImageUrl})\n" +
                                  $"*ID: {waifu.Id}*";
                    });
                }

                await context.Channel.SendMessageAsync("", embed: eb);
            }
        }

        private Waifu GetRandomFromBox()
        {
            return _boxCache[Rnd.Next(0, _boxCache.Count)];
        }

        private void AddWaifuToCache(Waifu waifu)
        {
            int amount = GetRarityAmount(waifu.Rarity);
            for (int i = 0; i < amount; i++)
            {
                _boxCache.Add(waifu);
            }
        }

        public static string GetRarityString(WaifuRarity rarity)
        {
            switch (rarity)
            {
                case WaifuRarity.Common:
                    return "Common";
                case WaifuRarity.Uncommon:
                    return "Uncommon";
                case WaifuRarity.Rare:
                    return "Rare";
                case WaifuRarity.Epic:
                    return "Epic";
                case WaifuRarity.UltimateWaifu:
                    return "Ultimate Waifu";
            }
            return "";
        }

        private WaifuRarity GetRarityByInt(int rarity)
        {
            switch (rarity)
            {
                case 0:
                    return WaifuRarity.Common;
                case 1:
                    return WaifuRarity.Uncommon;
                case 2:
                    return WaifuRarity.Rare;
                case 3:
                    return WaifuRarity.Epic;
                case 4:
                    return WaifuRarity.UltimateWaifu;
            }
            return WaifuRarity.Common;
        }

        private int GetRarityAmount(WaifuRarity rarity)
        {
            switch (rarity)
            {
                    case WaifuRarity.Common:
                        return 500;
                    case WaifuRarity.Uncommon:
                        return 300;
                    case WaifuRarity.Rare:
                        return 100;
                    case WaifuRarity.Epic:
                        return 50;
                    case WaifuRarity.UltimateWaifu:
                        return 10;
            }
            return 0;
        }
    }
}