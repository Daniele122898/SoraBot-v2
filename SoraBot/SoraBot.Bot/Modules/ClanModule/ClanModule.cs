using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using SoraBot.Bot.Models;
using SoraBot.Common.Extensions.Modules;
using SoraBot.Common.Utils;
using SoraBot.Data.Models.SoraDb;
using SoraBot.Data.Repositories.Interfaces;
using SoraBot.Services.Users;

namespace SoraBot.Bot.Modules.ClanModule
{
    [Name("Clan")]
    [Summary("All commands for clan functions")]
    public class ClanModule : SoraSocketCommandModule
    {
        private readonly IClanRepository _clanRepo;
        private readonly IUserService _userService;

        public ClanModule(IClanRepository clanRepo, IUserService userService)
        {
            _clanRepo = clanRepo;
            _userService = userService;
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
            var footer = RequestedByMe();
            var eb = new EmbedBuilder()
            {
                Footer = footer.WithText($"{footer.Text} | Created {clan.Created.ToString("dd/MM/yyyy")}"),
                Title = $"{INFO_EMOJI} {clan.Name} info",
                Color = Blue,
                Description = clan.Description ?? "_No description_"
            };
            
            if (!string.IsNullOrWhiteSpace(clan.AvatarUrl))
                eb.WithThumbnailUrl(clan.AvatarUrl);

            var members = await _clanRepo.GetClanMembers(clan.Id, 10);
            if (!members || members.Some().Count == 0)
            {
                await ReplyFailureEmbed("This clan somehow has no members...");
                return;
            }

            var totalExpTask = _clanRepo.GetClanTotalExp(clan.Id);
            foreach (var member in members.Some())
            {
                var user = await this._userService.GetOrSetAndGet(member.Id);
                string username = user.HasValue
                    ? Formatter.UsernameDiscrim(~user)
                    : member.Id.ToString(); 
                    
                eb.AddField(x =>
                {
                    x.Name = $"{(member.Id == clan.OwnerId ? "[O] " : "")}{username}";
                    x.IsInline = true;
                    x.Value = $"{member.Exp.ToString()} Exp";
                });
            }

            var total = await totalExpTask;
            eb.AddField(x =>
            {
                x.Name = "Total Clan";
                x.IsInline = false;
                x.Value = $"{total.ToString()} Exp";
            });

            await ReplyEmbed(eb);
        }
    }
}