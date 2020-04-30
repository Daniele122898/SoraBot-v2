using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using SoraBot.Common.Extensions.Modules;
using SoraBot.Data.Repositories.Interfaces;

namespace SoraBot.Bot.Modules
{
    public class StarboardModule : SoraSocketCommandModule
    {
        private readonly IStarboardRepository _starboardRepo;

        public StarboardModule(IStarboardRepository starboardRepo)
        {
            _starboardRepo = starboardRepo;
        }

        [Command("starboard"), Alias("star, starchannel, starboardchannel")]
        [Summary("This command allows you to set a starboard channel. " +
                 "You must be Administrator for this.")]
        public async Task SetStarboard(
            [Summary("Channel for Starboard. If no #channel is mentioned it will just use the current one")] 
            ISocketMessageChannel channel = null)
        {
            var starboard = channel ?? Context.Channel;
            if (!await UserHasGuildPermission(GuildPermission.Administrator)) return;

            await _starboardRepo.SetStarboardChannelId(Context.Guild.Id, starboard.Id).ConfigureAwait(false);
            await ReplySuccessEmbed($"Successfully set Starboard channel to #{starboard.Name}");
        }

        [Command("removestarboard"), Alias("rmstarboard", "rmstarchannel", "remove starboard")]
        [Summary("This command allows you to remove the starboard. This will not remove the channel or the messages " +
                 "but simply stop posting starred messages. You need Administrator for this.")]
        public async Task RemoveStarboard()
        {
            if (!await UserHasGuildPermission(GuildPermission.Administrator)) return;
            await _starboardRepo.RemoveStarboard(Context.Guild.Id).ConfigureAwait(false);
            await ReplySuccessEmbed("Successfully removed starboard :)");
        }

        [Command("starthreshold"), Alias("threshold", "minstars")]
        [Summary("This command allows you to set the minimum amount of star reactions needed " +
                 "so that the message gets added to the starboard. " +
                 "You need Administrator for this.")]
        public async Task SetThreshold(uint threshold)
        {
            if (threshold < 1)
            {
                await ReplyFailureEmbed("Threshold has to be greater than 0!");
                return;
            }
            if (!await UserHasGuildPermission(GuildPermission.Administrator)) return;
            await _starboardRepo.SetStarboardThreshold(Context.Guild.Id, threshold).ConfigureAwait(false);
            await ReplySuccessEmbed($"Successfully set threshold to {threshold.ToString()}");
        }
    }
}