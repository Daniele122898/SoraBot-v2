using System;
using System.Globalization;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using SoraBot.Common.Extensions.Modules;
using SoraBot.Common.Utils;
using Victoria;
using Victoria.Enums;

namespace SoraBot.Bot.Modules.AudioModule
{
    [Name("Music")]
    [Summary("All commands around music playing :>")]
    public partial class AudioModule : SoraSocketCommandModule
    {
        private readonly LavaNode _node;
        private readonly InteractiveService _interactiveService;

        public AudioModule(
            LavaNode node,
            InteractiveService interactiveService)
        {
            _node = node;
            _interactiveService = interactiveService;
        }
        
        [Command("soundcloud"), Alias("sc")]
        [Summary("Searches SoundCloud and gives you a list of possible songs")]
        public async Task ScSearch(
            [Summary("Name of the song you want to search"), Remainder]
            string query)
        {
            await this.SearchAndChoose(query.Trim(), false);
        }

        [Command("youtube"), Alias("yt")]
        [Summary("Searches YouTube and gives you a list of possible videos")]
        public async Task YtSearch(
            [Summary("Name of the video you want to search"), Remainder]
            string query)
        {
            await this.SearchAndChoose(query.Trim(), true);
        }

        [Command("shuffle"), Alias("shufflequeue", "shufflelist")]
        [Summary("Shuffles the queue")]
        public async Task ShuffleQueue()
        {
            if (!_node.TryGetPlayer(Context.Guild, out var player))
            {
                await ReplyFailureEmbed("I have not joined any Voice Channel yet.");
                return;
            }

            if (!await CheckIfSameVc(player.VoiceChannel))
                return;

            if (player.Queue.Count == 0)
            {
                await ReplyFailureEmbed("Queue empty. Nothing to shuffle");
                return;
            }

            player.Queue.Shuffle();
            await ReplyMusicEmbed("Shuffled queue for you ;)");
        }

        [Command("clear"), Alias("clearqueue", "clearlist")]
        [Summary("Cleares the entire music queue")]
        public async Task ClearQueue()
        {
            if (!_node.TryGetPlayer(Context.Guild, out var player))
            {
                await ReplyFailureEmbed("I have not joined any Voice Channel yet.");
                return;
            }

            if (!await CheckIfSameVc(player.VoiceChannel))
                return;

            player.Queue.Clear();
            await ReplyMusicEmbed("Cleared entire queue :)");
        }

        [Command("queue"), Alias("list", "musiclist")]
        [Summary("Displays the next couple songs in the queue")]
        public async Task ShowQueue()
        {
            if (!_node.TryGetPlayer(Context.Guild, out var player))
            {
                await ReplyFailureEmbed("I have not joined any Voice Channel yet.");
                return;
            }

            if (player.Track == null)
            {
                await ReplyFailureEmbed("I'm currently not playing anything");
                return;
            }

            var eb = new EmbedBuilder()
            {
                Color = Blue,
                Title = $"{MusicalNote} Queue",
                Footer = RequestedByMe()
            };

            eb.AddField(x =>
            {
                x.IsInline = false;
                x.Name = $"Now playing by {player.Track.Author}";
                x.Value =
                    $"[{Formatter.FormatTime(player.Track.Duration)}] - **[{player.Track.Title}]({player.Track.Url})**";
            });

            if (player.Queue.Count == 0)
            {
                await ReplyEmbed(eb);
                return;
            }

            // Otherwise built the rest of the queue
            int count = 0;
            foreach (var item in player.Queue.Items)
            {
                ++count;
                var track = (LavaTrack) item;
                eb.AddField(x =>
                {
                    x.IsInline = false;
                    // ReSharper disable once AccessToModifiedClosure
                    x.Name = $"#{count.ToString()} by {track.Author}";
                    x.Value = $"[{Formatter.FormatTime(track.Duration)}] - **[{track.Title}]({track.Url})**";
                });

                if (count >= 10)
                    break;
            }

            TimeSpan duration = new TimeSpan();

            foreach (var item in player.Queue.Items)
            {
                var track = (LavaTrack) item;
                duration = duration.Add(track.Duration);
            }

            duration = duration.Add(player.Track.Duration.Subtract(player.Track.Position));
            eb.AddField(x =>
            {
                x.IsInline = false;
                x.Name = $"{player.Queue.Count.ToString()} songs in queue";
                x.Value = $"[{Formatter.FormatTime(duration)}] total playtime";
            });

            await ReplyEmbed(eb);
        }

        [Command("nowplaying"), Alias("np", "now playing")]
        [Summary("Stats about the currently playing song.")]
        public async Task Np()
        {
            if (!_node.TryGetPlayer(Context.Guild, out var player))
            {
                await ReplyFailureEmbed("I have not joined any Voice Channel yet.");
                return;
            }

            if (player.Track == null)
            {
                await ReplyFailureEmbed("I'm currently not playing anything");
                return;
            }

            if (player.Track.IsStream)
            {
                await ReplyFailureEmbed("I cannot display information about a stream");
                return;
            }

            double percentageDone = (100.0 / player.Track.Duration.TotalSeconds) *
                                    player.Track.Position.TotalSeconds;
            int rounded = (int) Math.Floor(percentageDone / 10);
            string progress = "";
            for (int i = 0; i < 10; i++)
            {
                if (i == rounded)
                {
                    progress += " :red_circle: ";
                    continue;
                }

                progress += "▬";
            }

            var eb = new EmbedBuilder()
            {
                Color = Blue,
                Title = $"{MusicalNote} Currently playing by {player.Track.Author}",
                Description =
                    $"**[{player.Track.Title}]({player.Track.Url})**{(player.PlayerState == PlayerState.Paused ? " (Paused)" : "")}"
            };

            var imageUrl = await player.Track.FetchArtworkAsync();
            if (!string.IsNullOrWhiteSpace(imageUrl))
                eb.WithThumbnailUrl(imageUrl);

            eb.AddField(x =>
            {
                x.IsInline = false;
                x.Name = "Progress";
                x.Value =
                    $"[{Formatter.FormatTime(player.Track.Position)}] {progress} [{Formatter.FormatTime(player.Track.Duration)}]";
            });

            await ReplyEmbed(eb);
        }

        [Command("volume"), Alias("vol")]
        [Summary("Set the volume of the player to a value between 1 - 100")]
        public async Task SetVolume(
            [Summary("Value between 1 and 100")] uint vol)
        {
            if (!_node.TryGetPlayer(Context.Guild, out var player))
            {
                await ReplyFailureEmbed("I have not joined any Voice Channel yet.");
                return;
            }

            if (!await CheckIfSameVc(player.VoiceChannel))
                return;

            if (player.PlayerState != PlayerState.Playing)
            {
                await ReplyFailureEmbed("I'm currently not playing anything");
                return;
            }

            vol = Math.Clamp(vol, 1, 100);
            try
            {
                await player.UpdateVolumeAsync((ushort) vol);
                await ReplyMusicEmbed($"Set volume to {vol.ToString()}");
            }
            catch (Exception)
            {
                await ReplyFailureEmbed("Something broke :/");
            }
        }

        [Command("skip"), Alias("next")]
        [Summary("Skips the specified amount of songs")]
        public async Task SkipSong(
            [Summary("Number of songs to skip. 1 would just skip the currently playing song")]
            int number)
        {
            if (!_node.TryGetPlayer(Context.Guild, out var player))
            {
                await ReplyFailureEmbed("I have not joined any Voice Channel yet.");
                return;
            }

            if (!await CheckIfSameVc(player.VoiceChannel))
                return;

            if (player.Track == null)
            {
                await ReplyFailureEmbed("I'm currently not playing anything");
                return;
            }

            if (number < 1 || number > (player.Queue.Count + 1))
            {
                await ReplyFailureEmbed(
                    $"Cannot skip less than 1 song or more than the queue length + 1 ({(player.Queue.Count + 1).ToString()})");
                return;
            }

            int queueRemove = --number;
            for (int i = 0; i < queueRemove; i++)
            {
                player.Queue.TryDequeue(out _);
            }

            try
            {
                if (player.Queue.Count == 0)
                {
                    await player.StopAsync();
                    await ReplyMusicEmbed($"Queue is now finished.");
                    return;
                }

                var currentTrack = await player.SkipAsync();
                if (currentTrack == null)
                {
                    await ReplyMusicEmbed($"Queue is now finished.");
                }
                else
                {
                    await ReplyMusicExtended(currentTrack, false);
                }
            }
            catch (Exception)
            {
                await ReplyFailureEmbed("Something broke :/");
            }
        }

        [Command("seek")]
        [Summary("Seeks a certain amount in the track")]
        public async Task SeekSong(
            [Summary(
                "Time to seek. If just a number it will seek the amount in seconds. Otherwise you can use min:sec")]
            string seek)
        {
            if (!_node.TryGetPlayer(Context.Guild, out var player))
            {
                await ReplyFailureEmbed("I have not joined any Voice Channel yet.");
                return;
            }

            if (!await CheckIfSameVc(player.VoiceChannel))
                return;

            if (player.Track == null)
            {
                await ReplyFailureEmbed("I'm currently not playing anything");
                return;
            }

            var parsed = SeekParse(seek.Trim());
            if (!parsed.HasValue)
            {
                await ReplyFailureEmbedExtended(
                    "Failed to parse seek amount!",
                    "Make sure you either send just a number indicating the amount of seconds to skip or `min:sec`. For example `1:30`");
                return;
            }

            var skip = parsed.Value;

            if (skip.TotalSeconds < 1 || skip.TotalSeconds > (player.Track.Duration.TotalSeconds - 1))
            {
                await ReplyFailureEmbed($"Cannot seek shorter than 1 second or longer than " +
                                        $"track length ({(player.Track.Duration.TotalSeconds - 1).ToString(CultureInfo.InvariantCulture)} s).");
                return;
            }

            try
            {
                await player.SeekAsync(skip);
                await ReplyMusicEmbed($"Seeked song to {Formatter.FormatTime(skip)}");
            }
            catch (Exception)
            {
                await ReplyFailureEmbed("Something broke :/");
            }
        }

        [Command("skip"), Alias("next")]
        [Summary("Skips the current song and plays the next")]
        public async Task SkipSong()
        {
            if (!_node.TryGetPlayer(Context.Guild, out var player))
            {
                await ReplyFailureEmbed("I have not joined any Voice Channel yet.");
                return;
            }

            if (!await CheckIfSameVc(player.VoiceChannel))
                return;

            if (player.Track == null)
            {
                await ReplyFailureEmbed("I'm currently not playing anything");
                return;
            }

            try
            {
                if (player.Queue.Count == 0)
                {
                    await player.StopAsync();
                    await ReplyMusicEmbed($"Queue is now finished.");
                    return;
                }

                var currentTrack = await player.SkipAsync();
                if (currentTrack == null)
                {
                    await ReplyMusicEmbed($"Queue is now finished.");
                }
                else
                {
                    await ReplyMusicExtended(currentTrack, false);
                }
            }
            catch (Exception)
            {
                await ReplyFailureEmbed("Something broke :/");
            }
        }

        [Command("resume"), Alias("continue")]
        [Summary("Resumes music playback")]
        public async Task ResumePlayer()
        {
            if (!_node.TryGetPlayer(Context.Guild, out var player))
            {
                await ReplyFailureEmbed("I have not joined any Voice Channel yet.");
                return;
            }

            if (!await CheckIfSameVc(player.VoiceChannel))
                return;

            if (player.PlayerState == PlayerState.Playing)
            {
                await ReplyFailureEmbed("I'm already playing ...");
                return;
            }

            try
            {
                await player.ResumeAsync();
                await ReplyMusicEmbed($"Resumed {player.Track.Title}");
            }
            catch (Exception)
            {
                await ReplyFailureEmbed("Something broke :/");
            }
        }

        [Command("pause")]
        [Summary("Pauses the player.")]
        public async Task PausePlayer()
        {
            if (!_node.TryGetPlayer(Context.Guild, out var player))
            {
                await ReplyFailureEmbed("I have not joined any Voice Channel yet.");
                return;
            }

            if (!await CheckIfSameVc(player.VoiceChannel))
                return;

            if (player.PlayerState != PlayerState.Playing)
            {
                await ReplyFailureEmbed("I'm currently not playing anything");
                return;
            }

            try
            {
                await player.PauseAsync();
                await ReplyMusicEmbed($"Paused {player.Track.Title}");
            }
            catch (Exception)
            {
                await ReplyFailureEmbed("Something broke :/");
            }
        }

        [Command("play"), Alias("add")]
        public async Task Play(
            [Summary("Either add a link to what you want to play or if you just enter a search " +
                     "it will query YouTube and take the first result."), Remainder]
            string query)
        {
            if (!_node.TryGetPlayer(Context.Guild, out var player))
            {
                await ReplyFailureEmbed("I have not joined any Voice Channel yet.");
                return;
            }

            if (!await CheckIfSameVc(player.VoiceChannel))
                return;

            if (query.StartsWith("<") && query.EndsWith(">"))
                query = query.TrimStart('<').TrimEnd('>');

            bool isLink = Uri.IsWellFormedUriString(query, UriKind.RelativeOrAbsolute);

            var search = isLink
                ? await _node.SearchAsync(query)
                : await _node.SearchYouTubeAsync(query);

            if (search.LoadStatus == LoadStatus.LoadFailed
                || search.LoadStatus == LoadStatus.NoMatches
                || search.Tracks == null || search.Tracks.Count == 0)
            {
                await ReplyFailureEmbed("Couldn't find anything :/");
                return;
            }

            if (search.LoadStatus == LoadStatus.PlaylistLoaded)
            {
                int skip = -1;
                // First add selected track
                if (search.Playlist.SelectedTrack > 0 && search.Playlist.SelectedTrack < search.Tracks.Count)
                {
                    if (player.Track != null)
                        player.Queue.Enqueue(search.Tracks[search.Playlist.SelectedTrack]);
                    else
                        await player.PlayAsync(search.Tracks[search.Playlist.SelectedTrack]);
                    skip = search.Playlist.SelectedTrack;
                }

                // Add playlist
                for (var i = 0; i < search.Tracks.Count; i++)
                {
                    if (i == skip) continue;

                    var track = search.Tracks[i];
                    if (player.Track != null)
                        player.Queue.Enqueue(track);
                    else
                        await player.PlayAsync(track);
                }

                await ReplyMusicEmbed(
                    $"Successfully added {search.Tracks.Count.ToString()} songs from playlist {search.Playlist.Name}");
            }
            else
            {
                var track = search.Tracks[0];
                if (player.Track != null)
                {
                    player.Queue.Enqueue(track);
                    await ReplyMusicExtended(track);
                }
                else
                {
                    await player.PlayAsync(track);
                    await ReplyMusicExtended(track, false);
                }
            }
        }

        [Command("leave")]
        [Summary("Make sore leave your voice channel")]
        public async Task LeaveVC()
        {
            if (!_node.TryGetPlayer(Context.Guild, out var player))
            {
                await ReplyFailureEmbed("I'm not connected to any VC in this guild");
                return;
            }

            var playerVC = player.VoiceChannel;
            if (!await CheckIfSameVc(playerVC))
                return;

            try
            {
                await _node.LeaveAsync(playerVC);
            }
            catch (Exception)
            {
                await ReplyFailureEmbed("Something went wrong when i tried to leave :/");
            }
        }

        [Command("join")]
        [Summary("Make Sora join your voice channel")]
        public async Task Join()
        {
            if (_node.HasPlayer(Context.Guild))
            {
                await ReplyFailureEmbed("I'm already in another Voice Channel. Dont try to steal me >.<");
                return;
            }

            var voiceChannel = ((IGuildUser) Context.User).VoiceChannel;
            if (voiceChannel == null)
            {
                await ReplyFailureEmbed("You are currently not in a Voice Channel!");
                return;
            }

            try
            {
                await _node.JoinAsync(voiceChannel, (ITextChannel) Context.Channel);
            }
            catch (Exception)
            {
                await ReplyFailureEmbed("Failed to join your channel for some reason :/ Maybe i lack permission.");
            }
        }
    }
}