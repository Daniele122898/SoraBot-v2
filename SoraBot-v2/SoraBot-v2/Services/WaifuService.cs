using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
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
        rare				100 - 300
        epic				50 - 600
        ultimate Waifu		10 - 1500
        
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
        private readonly InteractiveService _interactive;

        public WaifuService(InteractiveService service)
        {
            _interactive = service;
        }
        
        private List<Waifu> _boxCache = new List<Waifu>();
        private List<Waifu> _specialWaifu = new List<Waifu>();

        private int BOX_COST = 500;
        private int SPECIAL_COST = 750;
        private byte BOX_CARD_AMOUNT = 3;
        
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
                    // special waifus should not be added to normal pot
                    if (amount == 0)
                    {
                        _specialWaifu.Add(waifu);
                        continue;
                    }
                    // add normal waifus to normal pot
                    for (int i = 0; i < amount; i++)
                    {
                        _boxCache.Add(waifu);
                    }
                }
                // shuffle for some extra RNG
                _boxCache.Shuffle();
                _boxCache.Shuffle();
            }
        }

        public async Task SetFavoriteWaifu(SocketCommandContext context, int waifuId)
        {
            using (var soraContext = new SoraContext())
            {
                var userdb = Utility.OnlyGetUser(context.User.Id, soraContext);
                if (userdb == null || userdb.UserWaifus.Count == 0)
                {
                    await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                        Utility.RedFailiureEmbed,
                        Utility.SuccessLevelEmoji[2],
                        "You have no Waifus to set as favorite! Open some WaifuBoxes!"
                    ).Build());
                    return;
                }
                // check if we have the specified waifu
                var waifu = userdb.UserWaifus.FirstOrDefault(x => x.WaifuId == waifuId);
                if (waifu == null)
                {
                    await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                        Utility.RedFailiureEmbed,
                        Utility.SuccessLevelEmoji[2],
                        $"You don't have that Waifu to set as favorite!"
                    ).Build());
                    return;
                }
                // set as favorite
                userdb.FavoriteWaifu = waifu.WaifuId;
                await soraContext.SaveChangesAsync();
                await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                    Utility.GreenSuccessEmbed,
                    Utility.SuccessLevelEmoji[0],
                    $"Successfully set favorite Waifu on profile."
                ).Build());
            }
        }

        public async Task MakeTradeOffer(SocketCommandContext context, SocketGuildUser other, int wantId, int offerId)
        {
            using (var soraContext = new SoraContext())
            {
                // check if they have ANY waifus at all
                var userdb = Utility.OnlyGetUser(context.User.Id, soraContext);
                if (userdb == null || userdb.UserWaifus.Count == 0)
                {
                    await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                        Utility.RedFailiureEmbed,
                        Utility.SuccessLevelEmoji[2],
                        "You have no waifus to trade! Open some WaifuBoxes!"
                    ).Build());
                    return;
                }

                var otherdb = Utility.OnlyGetUser(other.Id, soraContext);
                if (otherdb == null || otherdb.UserWaifus.Count == 0)
                {
                    await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                        Utility.RedFailiureEmbed,
                        Utility.SuccessLevelEmoji[2],
                        $"{other.Username} has no waifus to trade!"
                    ).Build());
                    return;
                }
                // check if both have the offered waifus. 
                // first other
                var otherWaifu = otherdb.UserWaifus.FirstOrDefault(x => x.WaifuId == wantId);
                if (otherWaifu == null)
                {
                    await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                        Utility.RedFailiureEmbed,
                        Utility.SuccessLevelEmoji[2],
                        $"{other.Username} doesn't have that waifu!"
                    ).Build());
                    return;
                }
                // now us
                var userWaifu = userdb.UserWaifus.FirstOrDefault(x => x.WaifuId == offerId);
                if (userWaifu == null)
                {
                    await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                        Utility.RedFailiureEmbed,
                        Utility.SuccessLevelEmoji[2],
                        $"You don't have that waifu to offer!"
                    ).Build());
                    return;
                }
                // now ask for the trade.
                var otherW = soraContext.Waifus.FirstOrDefault(x => x.Id == wantId);
                var userW = soraContext.Waifus.FirstOrDefault(x => x.Id == offerId);
                var eb = new EmbedBuilder()
                {
                    Title = "Waifu Trade Request",
                    Description = $"{context.User.Username} has requested to trade with you.",
                    Color = Utility.PurpleEmbed,
                    Footer = Utility.RequestedBy(context.User),
                    ImageUrl = userW.ImageUrl
                };

                eb.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = "User offers";
                    x.Value = $"{userW.Name}\n{GetRarityString(userW.Rarity)}\n*ID: {userW.Id}*";
                });
                
                eb.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = "User Wants";
                    x.Value = $"{otherW.Name}\n{GetRarityString(otherW.Rarity)}\n*ID: {otherW.Id}*";
                });
                
                eb.AddField(x =>
                {
                    x.IsInline = false;
                    x.Name = "Accept?";
                    x.Value = "You can accept this trade by writing `y` and decline by writing anything else.";
                });
                
                await context.Channel.SendMessageAsync("", embed: eb.Build());
                
                Criteria<SocketMessage> criteria = new Criteria<SocketMessage>();
                criteria.AddCriterion(new EnsureFromUserInChannel(other.Id, context.Channel.Id));

                var response = await _interactive.NextMessageAsync(context, criteria, TimeSpan.FromSeconds(45));
                
                if (response == null)
                {
                    await context.Channel.SendMessageAsync("", embed:
                        Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            $"{other.Username} didn't answer in time >.<").Build());
                    return;
                }

                if (!response.Content.Equals("y", StringComparison.OrdinalIgnoreCase))
                {
                    await context.Channel.SendMessageAsync("", embed:
                        Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            $"{other.Username} declined the trade offer!").Build());
                    return;
                }
                
                // accepted offer
                // add waifu
                GiveWaifuToId(userdb.UserId, otherW.Id, userdb);
                GiveWaifuToId(other.Id, userW.Id, otherdb);
                // remove waifu
                userWaifu.Count--;
                bool fav1 = false;
                bool fav2 = false;
                if (userWaifu.Count == 0)
                {
                    fav1 = RemoveWaifuFromUser(userdb, userWaifu);
                }

                otherWaifu.Count--;
                if (otherWaifu.Count == 0)
                {
                    fav2 = RemoveWaifuFromUser(otherdb, otherWaifu);
                }
                // completed trade
                await soraContext.SaveChangesAsync();
                string desc = "";
                if (fav1)
                {
                    desc +=
                        $"{context.User.Username}, you traded away your favorite Waifu. It has been removed from your profile.\n";
                }
                if (fav2)
                {
                    desc +=
                        $"{other.Username}, you traded away your favorite Waifu. It has been removed from your profile.";
                }
                var eb2 = Utility.ResultFeedback(
                    Utility.GreenSuccessEmbed,
                    Utility.SuccessLevelEmoji[0],
                    $"Successfully traded {userW.Name} for {otherW.Name}."
                );
                if (!string.IsNullOrWhiteSpace(desc))
                {
                    eb2.WithDescription(desc);
                }
                await context.Channel.SendMessageAsync("", embed: eb2.Build());
            }
        }

        public async Task SellDupes(SocketCommandContext context)
        {
            using (var soraContext = new SoraContext())
            {
                var userdb = Utility.OnlyGetUser(context.User.Id, soraContext);
                // check if user even has waifus
                if (userdb == null || userdb.UserWaifus.Count == 0)
                {
                    await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                        Utility.RedFailiureEmbed,
                        Utility.SuccessLevelEmoji[2],
                        "You have no waifus to sell! Open some WaifuBoxes!"
                    ).Build());
                    return;
                }
                // get all waifus that he has dupes of
                var dupes = userdb.UserWaifus.Where(x => x.Count > 1).ToList();
                // check if he has dupes
                if (dupes.Count == 0)
                {
                    await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                        Utility.RedFailiureEmbed,
                        Utility.SuccessLevelEmoji[2],
                        "You don't have any dupes at the moment."
                    ).Build());
                    return; 
                }
                // iterate through all the dupes
                int totalWaifus = 0;
                int totalCoins = 0;
                foreach (var waifu in dupes)
                {
                    // remove the waifus
                    int amount = waifu.Count - 1;
                    totalWaifus += amount;
                    waifu.Count -= amount;
                    // calculate coins
                    var w = soraContext.Waifus.FirstOrDefault(x => x.Id == waifu.WaifuId);
                    int cash = GetWaifuQuickSellCost(w?.Rarity ?? 0) * amount;
                    totalCoins += cash;
                }
                // add total coins to user
                userdb.Money += totalCoins;
                // save everything
                await soraContext.SaveChangesAsync();
                // report back to user
                await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                    Utility.GreenSuccessEmbed,
                    Utility.SuccessLevelEmoji[0],
                    $"You successfully sold {totalWaifus} Waifus for {totalCoins} Sora Coins!")
                    .Build());
            }
        }

        public async Task QuickSellWaifus(SocketCommandContext context, int waifuId, int amount)
        {
            using (var soraContext = new SoraContext())
            {
                var userdb = Utility.OnlyGetUser(context.User.Id, soraContext);
                if (userdb == null || userdb.UserWaifus.Count == 0)
                {
                    await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                        Utility.RedFailiureEmbed,
                        Utility.SuccessLevelEmoji[2],
                        "You have no waifus to sell! Open some WaifuBoxes!"
                    ).Build());
                    return;
                }

                var selected = userdb.UserWaifus.FirstOrDefault(x => x.WaifuId == waifuId);
                if (selected == null)
                {
                    await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                        Utility.RedFailiureEmbed,
                        Utility.SuccessLevelEmoji[2],
                        "Either this waifu doesn't exist or you don't own it!"
                    ).Build());
                    return; 
                }

                if (selected.Count < amount)
                {
                    await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                        Utility.RedFailiureEmbed,
                        Utility.SuccessLevelEmoji[2],
                        "You don't have enough of this Waifu. Sell less!"
                    ).Build());
                    return; 
                }

                selected.Count -= amount;
                var waifu = soraContext.Waifus.FirstOrDefault(x => x.Id == waifuId);
                int cash = GetWaifuQuickSellCost(waifu?.Rarity ?? 0) * amount;
                userdb.Money += cash;
                bool fav = false;
                if (selected.Count == 0)
                {
                    fav = RemoveWaifuFromUser(userdb, selected);
                }

                await soraContext.SaveChangesAsync();
                var eb = Utility.ResultFeedback(
                    Utility.GreenSuccessEmbed,
                    Utility.SuccessLevelEmoji[0],
                    $"You successfully sold {amount} for {cash} SC."
                );
                if (fav)
                {
                    eb.WithDescription("You sold your Favorite Waifu. Thus it has been removed from your profile.");
                }
                await context.Channel.SendMessageAsync("", embed: eb.Build());
            }
        }
        
        private bool RemoveWaifuFromUser(User userdb, UserWaifu waifu)
        {
            userdb.UserWaifus.Remove(waifu);
            if (waifu.WaifuId != userdb.FavoriteWaifu)
                return false;

            userdb.FavoriteWaifu = -1;
            return true;
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
                    ).Build());
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
                ).Build());
            }
        }

        public async Task UnboxSpecialWaifu(SocketCommandContext context)
        {
            using (var soraContext = new SoraContext())
            {
                var userdb = Utility.GetOrCreateUser(context.User.Id, soraContext);
                // check sora coins
                if (userdb.Money < SPECIAL_COST)
                {
                    await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                        Utility.RedFailiureEmbed,
                        Utility.SuccessLevelEmoji[2],
                        $"You don't have enough Sora Coins! You need {SPECIAL_COST} SC."
                    ).Build());
                    return;
                }
                // remove money
                userdb.Money -= SPECIAL_COST;
                // open box
                var waifus = new List<Waifu>();
                // one special waifu
                // this CANNOT be a dupe.
                var wspecial = GetRandomSpecialWaifu(userdb);
                // check if its null so he has ALL special waifus
                if (wspecial == null)
                {
                    await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                        Utility.RedFailiureEmbed,
                        Utility.SuccessLevelEmoji[2],
                        $"You already own ALL the available special waifus. Open normal boxes now."
                    ).Build());
                    return;
                }
                waifus.Add(wspecial);
                // add to user
                GiveWaifuToId(context.User.Id, wspecial.Id, userdb);
                // the others are normal waifus.
                for (int i = 0; i < BOX_CARD_AMOUNT-1; i++)
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
                    Description = $"You opened a Halloween WaifuBox for {SPECIAL_COST} SC.",
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

                await context.Channel.SendMessageAsync("", embed: eb.Build());
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
                    ).Build());
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
                    Description = $"You opened a regular WaifuBox for {BOX_COST} SC.",
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

                await context.Channel.SendMessageAsync("", embed: eb.Build());
            }
        }

        private Waifu GetRandomSpecialWaifu(User db)
        {
            // remove all Dupes from list
            var specialListWithoutDupes = _specialWaifu.Where(y => db.UserWaifus.All(x => x.WaifuId != y.Id)).ToList();
            // return null if he has all waifus so we can handle that case
            if (specialListWithoutDupes.Count == 0) return null;
            // otherwise give a random waifu of the remaining ones
            return specialListWithoutDupes[ThreadSafeRandom.ThisThreadsRandom.Next(0, specialListWithoutDupes.Count)];
        }

        private Waifu GetRandomFromBox()
        {
            return _boxCache[ThreadSafeRandom.ThisThreadsRandom.Next(0, _boxCache.Count)];
        }

        private void AddWaifuToCache(Waifu waifu)
        {
            int amount = GetRarityAmount(waifu.Rarity);
            for (int i = 0; i < amount; i++)
            {
                _boxCache.Add(waifu);
            }
            // reshuffle
            _boxCache.Shuffle();
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
                case WaifuRarity.Halloween:
                    return "Halloween";
            }
            return "";
        }

        private int GetWaifuQuickSellCost(WaifuRarity rarity)
        {
            switch (rarity)
            {
                case WaifuRarity.Common:
                    return 50;
                case WaifuRarity.Uncommon:
                    return 100;
                case WaifuRarity.Rare:
                    return 200;
                case WaifuRarity.Epic:
                    return 500;
                case WaifuRarity.UltimateWaifu:
                    return 1500;
                case WaifuRarity.Halloween:
                    return 300;
            }
            return 0;
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
                case 5:
                    return WaifuRarity.Halloween;
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
                        return 20;
                    case WaifuRarity.Halloween:
                        return 0;
            }
            return 0;
        }
    }
}