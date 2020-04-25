using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using SoraBot.Bot.Models;
using SoraBot.Common.Extensions.Modules;
using SoraBot.Common.Utils;
using SoraBot.Data.Dtos.Profile;
using SoraBot.Data.Repositories.Interfaces;
using SoraBot.Services.Cache;
using SoraBot.Services.Profile;

namespace SoraBot.Bot.Modules
{
    [Name("Profile")]
    [Summary("All commands concerning your profile card, level etc.")]
    public class ProfileModule : SoraSocketCommandModule
    {
        private readonly ImageGenerator _imgGen;
        private readonly IProfileRepository _profileRepo;
        private readonly ICacheService _cacheService;

        private const int _SET_BG_COOLDOWN_S = 45;
        private const string _SET_BG_CD_ID = "setbg:";

        public ProfileModule(ImageGenerator imgGen, IProfileRepository profileRepo, ICacheService cacheService)
        {
            _imgGen = imgGen;
            _profileRepo = profileRepo;
            _cacheService = cacheService;
        }

        private bool LinkIsNoImage(string url)
            => !url.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) &&
               !url.EndsWith(".png", StringComparison.OrdinalIgnoreCase) &&
               !url.EndsWith(".gif", StringComparison.OrdinalIgnoreCase) &&
               !url.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase);

        [Command("setbg"), Alias("setbackground", "sbg")]
        [Summary("Give Sora a link to an image or attach an image to set it as your " +
                 "profile card background")]
        public async Task SetBg(
            [Summary("Direct link to image. If you leave this blank you must provide an attachment!")]
            string url = null)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                if (Context.Message.Attachments.Count != 1)
                {
                    await ReplyFailureEmbed("If you do not provide an image link you MUST provide 1 Attached image");
                    return;
                }
                url = Context.Message.Attachments.First().Url;
            }
            // Check if URL is valid
            if (LinkIsNoImage(url))
            {
                await ReplyFailureEmbedExtended("The provided link or attachment is not an image!",
                    "Make sure the link ends with any of these extensions: `.jpg, .png, .gif, .jpeg`");
                return;
            }
            // Check cooldown
            var cd = _cacheService.Get<DateTime>(_SET_BG_CD_ID + Context.User.Id.ToString());
            if (cd.HasValue)
            {
                var secondsRemaining = cd.Value.Subtract(DateTime.UtcNow.TimeOfDay).Second;
                await ReplyFailureEmbed(
                    $"Dont break me >.< Please wait another {secondsRemaining.ToString()} seconds!");
                return;
            }
        }

        [Command("profile"), Alias("p")]
        [Summary("Shows your or the @mentioned user's profile card with level and rank stats")]
        public async Task GenerateImage(
            [Summary("@User or leave blank to get your own")]
            DiscordGuildUser userT = null)
        {
            var user = userT?.GuildUser ?? (IGuildUser)Context.User;
            string imageGen = Path.Combine(Directory.GetCurrentDirectory(), "ImageGenerationFiles");
            string filePath = Path.Combine(imageGen, "ProfileCards",
                $"{user.Id.ToString()}_profileCard.png");
            string bgPath = Path.Combine(imageGen, "ProfileCreation", "defaultBG.png");
            string avatarPath = Path.Combine(imageGen, "AvatarCache", "avatar.png");

            var userStatsM = await _profileRepo.GetProfileStatistics(user.Id, Context.Guild.Id).ConfigureAwait(false);
            if (!userStatsM.HasValue)
            {
                await ReplyFailureEmbed(
                    $"{Formatter.UsernameDiscrim(user)} is not in my Database :/ Make sure he used or chatted with Sora at least once.");
                return;
            }

            var us = userStatsM.Value;
            var lvl = ExpService.CalculateLevel(us.GlobalExp);
            _imgGen.GenerateProfileImage(new ProfileImageGenDto()
            {
                BackgroundPath = bgPath,
                AvatarPath = avatarPath,
                Name = user.Username,
                GlobalExp = us.GlobalExp,
                GlobalLevel = lvl,
                GlobalRank = us.GlobalRank,
                GlobalNextLevelExp = ExpService.CalculateNeededExp(lvl+1)
            }, filePath);
            await Context.Channel.SendFileAsync(filePath);
        }
    }
}