using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using SoraBot.Bot.Models;
using SoraBot.Common.Extensions.Modules;
using SoraBot.Common.Utils;
using SoraBot.Data.Models.SoraDb;
using SoraBot.Data.Repositories.Interfaces;

namespace SoraBot.Bot.Modules.ClanModule
{
    [Name("Clan")]
    [Summary("All commands for clan functions")]
    public class ClanModule : SoraSocketCommandModule
    {
        private readonly IClanRepository _clanRepo;

        public ClanModule(IClanRepository clanRepo)
        {
            _clanRepo = clanRepo;
        }
        
        [Command("claninfo"), Alias("cinfo", "clan info")]
        [Summary("Get info about the clan by name")]
        public async Task GetClanInfoByName(
            [Summary("Name of the clan") , Remainder] 
            string name)
        {
            var clan = await _clanRepo.GetClanByName(name);
            if (!clan)
            {
                await ReplyFailureEmbed("Clan with that name does not exist.");
                return;
            }

            await this.PrintClanInfo(clan.Some());
        }
        
        [Command("claninfo"), Alias("cinfo", "clan info")]
        [Summary("Get info about the clan your in or by mentioning a user")]
        public async Task GetClanInfoByName(
            [Summary("@mention user or leave blank to get your own info")]
            DiscordUser userT = null)
        {
            var user = userT?.User ?? Context.User;

            var clan = await _clanRepo.GetClanByUserId(user.Id);
            if (!clan)
            {
                await ReplyFailureEmbed($"{Formatter.UsernameDiscrim(user)} is not in a clan.");
                return;
            }

            await this.PrintClanInfo(clan.Some());
        }

        private async Task PrintClanInfo(Clan clan)
        {
            var eb = new EmbedBuilder()
            {
                Footer = RequestedByMe(),
                Title = $"{INFO_EMOJI} {clan.Name} info",
                Color = Blue,
                Description = clan.Description ?? "_No description_"
            };
            
            if (!string.IsNullOrWhiteSpace(clan.AvatarUrl))
                eb.WithThumbnailUrl(clan.AvatarUrl);

            var members = await _clanRepo.GetClanMembers(clan.Id, 10);
            
            
        }
    }
}