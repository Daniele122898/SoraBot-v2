using System;
using System.Linq;
using System.Threading.Tasks;
using ArgonautCore.Lw;
using Discord;
using SoraBot.Bot.Extensions.Interactive;
using SoraBot.Common.Utils;
using Victoria;
using Victoria.Enums;

namespace SoraBot.Bot.Modules.AudioModule
{
    public partial class AudioModule
    {
        private async Task SearchAndChoose(string query, bool yt)
        {
            if (!_node.TryGetPlayer(Context.Guild, out var player))
            {
                await ReplyFailureEmbed("I have not joined any Voice Channel yet.");
                return;
            }

            if (!await CheckIfSameVc(player.VoiceChannel))
                return;

            var search = yt ? await _node.SearchYouTubeAsync(query) : await _node.SearchSoundCloudAsync(query);
            if (search.LoadStatus == LoadStatus.LoadFailed || search.LoadStatus == LoadStatus.NoMatches)
            {
                await ReplyFailureEmbed("Couldn't find anything with the specified query.");
                return;
            }

            // Create selection
            var eb = new EmbedBuilder()
            {
                Color = Blue,
                Title = $"{MusicalNote} Top Search Results",
                Description = "Answer with the index of the song you'd like to add. Anything else to choose nothing",
                Footer = RequestedByMe()
            };

            var tracks = search.Tracks.Take(Math.Min(10, search.Tracks.Count)).ToList();
            for (var i = 0; i < tracks.Count; i++)
            {
                var track = tracks[i];
                eb.AddField(x =>
                {
                    x.IsInline = false;
                    // ReSharper disable once AccessToModifiedClosure
                    x.Name = $"#{(i + 1).ToString()} by {track.Author}";
                    x.Value = $"[{Formatter.FormatTime(track.Duration)}] - **[{track.Title}]({track.Url})**";
                });
            }

            var msg = await ReplyEmbed(eb);
            var criteria =
                InteractiveServiceExtensions.CreateEnsureFromUserInChannelCriteria(Context.User.Id, Context.Channel.Id);
            var resp = await _interactiveService.NextMessageAsync(Context, criteria, TimeSpan.FromSeconds(45));
            await msg.DeleteAsync(); // To reduce clutter
            if (resp == null)
            {
                await ReplyFailureEmbed("Failed to answer in time >.<");
                return;
            }

            if (!int.TryParse(resp.Content, out var trackNr))
            {
                await ReplyFailureEmbed("Please respond with the ID of the song to add e.g. `1` or `7`.");
                return;
            }

            trackNr--;
            if (trackNr < 0 || trackNr >= tracks.Count)
            {
                await ReplyFailureEmbed(
                    $"Not a valid ID! Please choose a song between 1 and {tracks.Count.ToString()}");
                return;
            }

            var t = tracks[trackNr];
            try
            {
                // We got a track to add or play
                if (player.Track == null)
                {
                    await player.PlayAsync(t);
                    await ReplyMusicExtended(t, false);
                    return;
                }

                // Otherwise we add it to the queue
                player.Queue.Enqueue(t);
                await ReplyMusicExtended(t);
            }
            catch (Exception)
            {
                await ReplyFailureEmbed("Something broke :/");
            }
        }

        private static Option<TimeSpan> SeekParse(string seek)
        {
            if (!seek.Contains(":"))
            {
                // Try parse as seconds
                if (!int.TryParse(seek, out var seconds))
                    return Option.None<TimeSpan>();

                return TimeSpan.FromSeconds(seconds);
            }

            // Parse as minute and second string :D
            var split = seek.Split(":");
            if (split.Length != 2)
                return Option.None<TimeSpan>();

            // Otherwise try to parse both values
            if (!int.TryParse(split[0], out var minutes) || !int.TryParse(split[1], out var secs))
                return Option.None<TimeSpan>();

            return TimeSpan.FromMinutes(minutes).Add(TimeSpan.FromSeconds(secs));
        }

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