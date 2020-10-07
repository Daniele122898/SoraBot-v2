using System;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using SoraBot.Bot.Extensions.Interactive;
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
        private readonly InteractiveService _interactiveService;

        public MarriageModule(IMarriageRepository marriageRepo, IUserService userService, InteractiveService interactiveService)
        {
            _marriageRepo = marriageRepo;
            _userService = userService;
            _interactiveService = interactiveService;
        }

        [Command("divorce")]
        [Summary("Divorce a user by his UserId in case you cannot @ them anymore")]
        public async Task DivorceId(ulong id)
        {
            await this.Divorce(id);
        }
        
        [Command("divorce")]
        [Summary("Divorce a user by @mentioning them")]
        public async Task DivorceMention(DiscordGuildUser user)
        {
            await this.Divorce(user.GuildUser.Id);
        }

        private async Task Divorce(ulong id)
        {
            if (!await _marriageRepo.TryDivorce(Context.User.Id, id))
            {
                await ReplyFailureEmbed("You are not married to that user");
                return;
            }

            await ReplySuccessEmbed("You have been successfully divorced");
        }

        [Command("marry")]
        [Summary("Ask the @mentioned person to marry you")]
        public async Task Marry(DiscordGuildUser user)
        {
            await ReplyEmbed(
                $"{Formatter.UsernameDiscrim(user.GuildUser)}, do you want to marry {Formatter.UsernameDiscrim(Context.User)}?",
                Purple, "💍");

            var criteria =
                InteractiveServiceExtensions.CreateEnsureFromUserInChannelCriteria(user.GuildUser.Id,
                    Context.Channel.Id);
            var resp = await _interactiveService.NextMessageAsync(Context, criteria, TimeSpan.FromSeconds(45)).ConfigureAwait(false);
            if (resp == null)
            {
                await ReplyFailureEmbed($"{Formatter.UsernameDiscrim(user.GuildUser)} didn't answer in time >.<");
                return;
            }

            if (!InteractiveServiceExtensions.StringContainsYes(resp.Content))
            {
                await ReplyFailureEmbed($"{Formatter.UsernameDiscrim(user.GuildUser)} didn't answer with a yes ˚‧º·(˚ ˃̣̣̥᷄⌓˂̣̣̥᷅ )‧º·˚");
                return;
            }
            
            var res = await _marriageRepo.TryAddMarriage(Context.User.Id, user.GuildUser.Id);
            if (!res)
            {
                await ReplyFailureEmbed(res.Err().Message);
                return;
            }
            var eb = new EmbedBuilder()
            {
                Color = Purple,
                Title = "💑 You are now married",
                ImageUrl = "https://media.giphy.com/media/iQ5rGja9wWB9K/giphy.gif"
            };

            await ReplyEmbed(eb);
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
                var partnerId = marriage.Partner1Id == user.Id ? marriage.Partner2Id : marriage.Partner1Id;
                var u = await _userService.GetOrSetAndGet(partnerId);
                var name = u ? Formatter.UsernameDiscrim((~u)) : partnerId.ToString();

                eb.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = name;
                    x.Value = $"*Since {marriage.PartnerSince:dd/MM/yyyy}*{(adv ? $"\nID: {partnerId.ToString()}" : "")}";
                });
            }

            await ReplyEmbed(eb);
        }
    }
}