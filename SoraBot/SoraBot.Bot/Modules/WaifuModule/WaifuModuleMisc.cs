using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using SoraBot.Common.Utils;
using SoraBot.Data.Models.SoraDb;
using SoraBot.Services.Waifu;

namespace SoraBot.Bot.Modules.WaifuModule
{
    public partial class WaifuModule
    {
        [Command("waifustats"), Alias("wstats")]
        [Summary("Shows your how many waifus you got and your completion %")]
        public async Task GetWaifuStats(IUser userT = null)
        {
            var user = userT ?? Context.User;
            var waifus = await _waifuService.GetAllWaifusFromUser(user.Id).ConfigureAwait(false);
            if (waifus == null || waifus.Count == 0)
            {
                await ReplyFailureEmbed($"{(userT == null ? "You have" : $"{Formatter.UsernameDiscrim(user)} has")} no waifus.");
                return;
            }

            // Unlike the total waifus we actually cache this value for 1 hour. This is because it's less important and can be inaccurate for a while
            // This frees up resources for more important tasks
            var allRarityStats = await _waifuService.GetTotalWaifuRarityStats().ConfigureAwait(false);
            
            var eb = new EmbedBuilder()
            {
                Color = Purple,
                Footer = RequestedByFooter(Context.User),
                ThumbnailUrl = user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl(),
                Title = "Waifu Stats",
                Description = "This shows you how many waifus you own of each category and your completion percentage." +
                              " This does not take dupes into account!"
            };

            int totalHas = 0;
            var userRarity = waifus.GroupBy(w => w.Rarity, (rarity, ws) => new {rarity, count = ws.Count()})
                .ToDictionary(x=> x.rarity, x => x.count);

            // This line kinda sucks. Don't know if there is a better way to solve this but that aint it chief
            var allRarities = ((WaifuRarity[]) Enum.GetValues(typeof(WaifuRarity))).OrderBy(x => x).ToList();
            int total = 0;
            foreach (var rarity in allRarities)
            {
                userRarity.TryGetValue(rarity, out int count);
                
                int allRar = allRarityStats[rarity];
                total += allRar;
                
                eb.AddField(x =>
                {
                    x.Name = WaifuFormatter.GetRarityString(rarity);
                    x.IsInline = true;
                    x.Value = $"{count.ToString()} / {allRar.ToString()} ({((float) count / allRar):P2})";
                });
                totalHas += count;
            }
            
            eb.AddField(x =>
            {
                x.Name = "Total";
                x.IsInline = true;
                x.Value = $"{totalHas.ToString()} / {total.ToString()} ({((float) totalHas / total):P2})";
            });

            await ReplyAsync("", embed: eb.Build());
        }

        [Command("mywaifus"), Alias("waifus")]
        [Summary("Links you to a page where you can see all of your owned waifus or the person you @mentioned")]
        public async Task ShowMyWaifus(IUser userT = null)
        {
            var user = userT ?? Context.User;
            await ReplyAsync($"Check out **{user.Username}'s Waifus** here: https://sorabot.pw/user/{user.Id}/waifus °˖✧◝(⁰▿⁰)◜✧˖°");
        }
        
        [Command("allwaifus"), Alias("waifulist", "wlist")]
        [Summary("Links a page that shows all the waifus that exist")]
        public async Task ShowAllWaifus()
            => await ReplyAsync($"Check out **all Waifus** here: https://sorabot.pw/allwaifus °˖✧◝(⁰▿⁰)◜✧˖°");
    }
}