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
            var allWaifus = await _waifuService.GetAllWaifus().ConfigureAwait(false);

            var allRarityStats = allWaifus.GroupBy(w => w.Rarity, (rarity, ws) => new {rarity, count = ws.Count()})
                .ToDictionary(x=> x.rarity, x => x.count);
            
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

            var allRarities = ((WaifuRarity[]) Enum.GetValues(typeof(WaifuRarity))).OrderBy(x => x).ToList();
            foreach (var rarity in allRarities)
            {
                userRarity.TryGetValue(rarity, out int count);
                
                eb.AddField(x =>
                {
                    x.Name = WaifuFormatter.GetRarityString(rarity);
                    x.IsInline = true;
                    x.Value = $"{count.ToString()} / {allRarityStats[rarity].ToString()} ({((float) count / allRarityStats[rarity]):P2})";
                });
                totalHas += count;
            }
            
            eb.AddField(x =>
            {
                x.Name = "Total";
                x.IsInline = true;
                x.Value = $"{totalHas.ToString()} / {allWaifus.Count.ToString()} ({((float) totalHas / allWaifus.Count):P2})";
            });

            await ReplyAsync("", embed: eb.Build());
        }
    }
}