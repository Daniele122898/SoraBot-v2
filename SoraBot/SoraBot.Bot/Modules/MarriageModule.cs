using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using SoraBot.Bot.Models;
using SoraBot.Common.Extensions.Modules;
using SoraBot.Common.Utils;
using SoraBot.Data.Repositories.Interfaces;
using SoraBot.Services.Users;

namespace SoraBot.Bot.Modules
{
    public class MarriageModule : SoraSocketCommandModule
    {
        private readonly IMarriageRepository _marriageRepo;
        private readonly IUserService _userService;

        public MarriageModule(IMarriageRepository marriageRepo, IUserService userService)
        {
            _marriageRepo = marriageRepo;
            _userService = userService;
        }

        [Command("marriages")]
        [Summary("Gives you a list of all the marriages of the specified user. " +
                 "Shows your own marriages if no user was @mentioned")]
        public async Task GetAllMarriages(DiscordGuildUser userT = null)
        {
            await this.GetAllMarriagesAdv(userT);
        }
        
        [Command("marriagesext"), Alias("marriages ext", "marriages extended")]
        [Summary("Gives you a list of all the marriages of the specified user. " +
                 "Shows your own marriages if no user was @mentioned. " +
                 "This extended version always shows the ID of the partner to be able to divorce if the user is no longer @mentionable")]
        public async Task GetAllMarriagesExt(DiscordGuildUser userT = null)
        {
            await this.GetAllMarriagesAdv(userT, true);
        }

        private async Task GetAllMarriagesAdv(DiscordGuildUser userT = null, bool adv = false)
        {
            var user = userT?.GuildUser ?? (IGuildUser) Context.User;
            var marriages = await _marriageRepo.GetAllMarriagesOfUser(user.Id);
            if (!marriages)
            {
                await ReplyFailureEmbed($"{Formatter.UsernameDiscrim(user)} has no marriages");
                return;
            }
            
            var eb = new EmbedBuilder()
            {
                Color = Purple,
                Title = $"💕 Marriages of {Formatter.UsernameDiscrim(user)}",
                ThumbnailUrl = user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl(),
                Footer = RequestedByMe()
            };

            var marr = marriages.Some();
            foreach (var marriage in marr)
            {
                var partnerId = marriage.Partner1 == user.Id ? marriage.Partner2 : marriage.Partner1;
                var u = await _userService.GetOrSetAndGet(partnerId);
                var name = u ? Formatter.UsernameDiscrim((~u)) : partnerId.ToString();

                eb.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = name;
                    x.Value = $"*Since {marriage.PartnerSince:dd/MM/yyyy}*{(adv ? $"ID: {partnerId.ToString()}" : "")}";
                });
            }

            await ReplyEmbed(eb);
        }
    }
}