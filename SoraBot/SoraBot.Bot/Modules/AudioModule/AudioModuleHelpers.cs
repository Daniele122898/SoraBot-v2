using System.Threading.Tasks;
using Discord;
using Victoria;

namespace SoraBot.Bot.Modules.AudioModule
{
    public partial class AudioModule
    {
        private async Task<bool> CheckIfSameVc(IVoiceChannel playerVC)
        {
            var userVC = ((IGuildUser) Context.User).VoiceChannel;
            if (playerVC == null || userVC == null)
            {
                await ReplyFailureEmbed("You're not connected to a voice channel!");
                return false;
            }
            if (playerVC.Id != userVC.Id)
            {
                await ReplyFailureEmbed("I'm not in the same voice channel as you.");
                return false;
            }
            return true;
        }

        private async Task ReplyMusicExtended(LavaTrack track, bool added = true)
        {
            var url = await track.FetchArtworkAsync();
            await ReplyMusicEmbedExtended(
                track.Title,
                track.Author,
                url,
                track.Duration.ToString(@"mm\:ss"),
                track.Url,
                added);
        }
    }
}