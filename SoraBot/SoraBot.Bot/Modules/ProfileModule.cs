using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using SoraBot.Bot.Models;
using SoraBot.Common.Extensions.Modules;
using SoraBot.Services.Profile;

namespace SoraBot.Bot.Modules
{
    [Name("Profile")]
    [Summary("All commands concerning your profile card, level etc.")]
    public class ProfileModule : SoraSocketCommandModule
    {
        private readonly ImageGenerator _imgGen;

        public ProfileModule(ImageGenerator imgGen)
        {
            _imgGen = imgGen;
        }

        [Command("p")]
        public async Task GenerateImage(DiscordGuildUser userT = null)
        {
            var user = userT?.GuildUser ?? (IGuildUser)Context.User;
            string imageGen = Path.Combine(Directory.GetCurrentDirectory(), "ImageGenerationFiles");
            string filePath = Path.Combine(imageGen, "ProfileCards",
                $"{user.Id.ToString()}_profileCard.png");
            string bgPath = Path.Combine(imageGen, "ProfileCreation", "defaultBG.png");
            string avatarPath = Path.Combine(imageGen, "AvatarCache", "avatar.png");
            _imgGen.GenerateProfileImage(new ProfileImageGenConfig()
            {
                BackgroundPath = bgPath,
                AvatarPath = avatarPath,
                Name = user.Username,
                GlobalExp = 1000,
                GlobalLevel = 2,
                GlobalRank = 1,
                GlobalNextLevelExp = 2000
            }, filePath);
            await Context.Channel.SendFileAsync(filePath);
        }
    }
}