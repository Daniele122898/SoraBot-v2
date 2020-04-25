using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using SoraBot.Bot.Models;
using SoraBot.Common.Extensions.Modules;
using SoraBot.Common.Utils;
using SoraBot.Data.Dtos.Profile;
using SoraBot.Data.Repositories.Interfaces;
using SoraBot.Services.Profile;

namespace SoraBot.Bot.Modules
{
    [Name("Profile")]
    [Summary("All commands concerning your profile card, level etc.")]
    public class ProfileModule : SoraSocketCommandModule
    {
        private readonly ImageGenerator _imgGen;
        private readonly IProfileRepository _profileRepo;

        public ProfileModule(ImageGenerator imgGen, IProfileRepository profileRepo)
        {
            _imgGen = imgGen;
            _profileRepo = profileRepo;
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