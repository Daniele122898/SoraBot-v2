using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using SoraBot_v2.Data;
using Victoria;
using Victoria.Objects;
using Victoria.Objects.Enums;
using Discord.Addons.Interactive;

namespace SoraBot_v2.Services
{
    public class AudioService
    {

        private LavaNode _lavaNode;
        private InteractiveService _interactive;
        private readonly ConcurrentDictionary<ulong, (LavaTrack track, List<ulong> votes)> _voteSkip;

        public AudioService(InteractiveService service)
        {
            _interactive = service;
        }
        
        public void Initialize(LavaNode node)
        {
            _lavaNode = node;
            // lavanode events
            node.Stuck += NodeOnStuck;
            node.Finished += NodeOnFinished;
            node.Exception += NodeOnException;
            node.Updated += NodeOnUpdated;
        }

        public async Task ConnectAsync(ulong guildId, IVoiceState state, IMessageChannel channel)
        {
            if (state.VoiceChannel == null)
            {
                await channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                        Utility.RedFailiureEmbed,
                        Utility.SuccessLevelEmoji[2],
                        "You aren't connected to any voice channels.")
                    .Build());
                return;
            }

            var player = await _lavaNode.JoinAsync(state.VoiceChannel, channel);
            await channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                Utility.GreenSuccessEmbed,
                Utility.SuccessLevelEmoji[0],
                $"Connected to {state.VoiceChannel}.")
                .Build());
        }

        private void loadPlaylist(LavaPlayer player, IEnumerable<LavaTrack> tracks)
        {
            // load in playlist
            foreach (LavaTrack track in tracks)
            {
                try
                {
                    if (player.CurrentTrack != null)
                        player.Enqueue(track);
                    else
                        player.Play(track);
                }
                catch
                {
                    continue;
                }
            }         
        }

        private async Task<(LavaTrack track, string reason)> GetSongFromSelect(SocketCommandContext context, LavaResult search)
        {
             // now lets build the embed to ask the user what to use
            EmbedBuilder eb = new EmbedBuilder()
            {
                Color = Utility.BlueInfoEmbed,
                Title = "Top Search Results",
                Description = "Send index of the track you want.",
                Footer = Utility.RequestedBy(context.User)
            };

            int maxCount = (search.Tracks.Count() < 10 ? search.Tracks.Count() : 10);
            int count = 1;

            foreach (LavaTrack track in search.Tracks)
            {
                eb.AddField(x =>
                {
                    x.IsInline = false;
                    x.Name = $"#{count} by {track.Author}";
                    x.Value = $"[{track.Length.ToString(@"mm\:ss")}] - **[{track.Title}]({track.Uri})**";
                });

                count++;
                if (count >= maxCount) break;
            }
            
            var msg = await context.Channel.SendMessageAsync("", embed: eb.Build());
            var response =
                await _interactive.NextMessageAsync(context, true, true, TimeSpan.FromSeconds(45));
            await msg.DeleteAsync();
            if (response == null)
                return (null, $"{Utility.GiveUsernameDiscrimComb(context.User)} did not reply :/");
            
            if (!Int32.TryParse(response.Content, out var index))
                return (null, "Only send the Index!");

            if (index > maxCount || index < 1)
                return (null, "Invalid Index!");

            LavaTrack finalTrack = search.Tracks.ElementAt(index-1);
            return (finalTrack, null);
        }

        public async Task YoutubeOrSoundCloudSearch(SocketCommandContext context, string query, bool youtube)
        {
            var search = youtube ? await _lavaNode.SearchYouTubeAsync(query) : await _lavaNode.SearchSoundCloudAsync(query);
            if (search.LoadResultType == LoadResultType.NoMatches || search.LoadResultType == LoadResultType.LoadFailed)
            {
                await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                        Utility.RedFailiureEmbed,
                        Utility.SuccessLevelEmoji[2],
                        "Couldn't find anything.")
                    .Build());
                return;
            }

            var result = await GetSongFromSelect(context, search);

            if (result.track == null)
            {
                await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                    Utility.RedFailiureEmbed, 
                    Utility.SuccessLevelEmoji[2], 
                    result.reason)
                    .Build());
                return;
            }
            
            var player = _lavaNode.GetPlayer(context.Guild.Id);
            bool queued = false;
            if (player.CurrentTrack != null)
            {
                player.Enqueue(result.track);
                queued = true;
            }
            else
                player.Play(result.track);
            
            await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                    Utility.GreenSuccessEmbed,
                    Utility.MusicalNote,
                    $"{(queued ? "Enqueued" : "Playing")}: [{result.track.Length.ToString(@"mm\:ss")}] - **{result.track.Title}**")
                .WithUrl(result.track.Uri.ToString())
                .Build());
        }

        public async Task<(LavaTrack track, bool enqued, string name, int num)> PlayAsync(ulong guildId, string query)
        {
            // if url get that otherwise search yt
            bool isLink = Uri.IsWellFormedUriString(query, UriKind.RelativeOrAbsolute);
            
            var search = isLink
                ? await _lavaNode.GetTracksAsync(query)
                : await _lavaNode.SearchYouTubeAsync(query);
            
            if (search.LoadResultType == LoadResultType.NoMatches || search.LoadResultType == LoadResultType.LoadFailed)
            {
                return (null, false, null, 0);
            }

            if (!isLink || search.Tracks.Count() == 1)
            {
                // get first track
                var track = search.Tracks.FirstOrDefault();
                var player = _lavaNode.GetPlayer(guildId);
                if (player.CurrentTrack != null)
                {
                    player.Enqueue(track);
                    return (track, true, null, 1);
                }
                player.Play(track);
                return (track, false, null, 1);
            }
            
            // get playlist
            loadPlaylist(_lavaNode.GetPlayer(guildId), search.Tracks);
            return (null, false, search.PlaylistInfo.Name, search.Tracks.Count());
        }

        public async Task<string> DisconnectAsync(ulong guildId)
            => await _lavaNode.LeaveAsync(guildId) ? "Disconnected." : "Not connected to any voice channels.";

        public string Pause(ulong guildId)
        {
            var player = _lavaNode.GetPlayer(guildId);
            if(player?.CurrentTrack == null) return "Not playing anything currently.";
            player.Pause();
            return $"Paused: {player.CurrentTrack.Title}";
        }

        public string Resume(ulong guildId)
        {
            var player = _lavaNode.GetPlayer(guildId);
            if(player?.CurrentTrack == null) return "Not playing anything currently.";
            player.Resume();
            return $"Resumed: {player.CurrentTrack.Title}";
        }

        public Embed NowPlaying(ulong guildId)
        {
            var player = _lavaNode.GetPlayer(guildId);
            if(player?.CurrentTrack == null) return Utility.ResultFeedback(
                Utility.BlueInfoEmbed,
                Utility.MusicalNote,
                "Not playing anything currently.").Build();

            if (player.CurrentTrack.IsStream)
            {
                return Utility.ResultFeedback(
                    Utility.BlueInfoEmbed,
                    Utility.MusicalNote,
                    "Cannot display more info of a stream").Build();
            }

            double percentageDone = (100.0 / player.CurrentTrack.Length.TotalSeconds) *
                                    player.Position.TotalSeconds;

            int rounded = (int) Math.Floor(percentageDone / 10);
            string progress = "";
            for (int i = 0; i < 10; i++)
            {
                if (i == rounded) {
                    progress += " :red_circle: ";
                    continue;
                }
                progress += "▬";
            }

            return Utility.ResultFeedback(
                Utility.BlueInfoEmbed,
                Utility.MusicalNote,
                $"Currently playing by {player.CurrentTrack.Author}")
                .WithDescription($"**[{player.CurrentTrack.Title}]({player.CurrentTrack.Uri})**")
                .AddField(x =>
                {
                    x.IsInline = false;
                    x.Name = "Progress";
                    x.Value =
                        $"[{player.Position.ToString(@"mm\:ss")}] {progress} [{player.CurrentTrack.Length.ToString(@"mm\:ss")}]";
                })
                .Build();    
        }

        public Embed DisplayQueue(ulong guildId, SocketUser user, IMessageChannel channel)
        {
            var player = _lavaNode.GetPlayer(guildId);
            if(player?.CurrentTrack == null) return Utility.ResultFeedback(
                Utility.BlueInfoEmbed,
                Utility.MusicalNote,
                "Not playing anything currently.").Build();
            
            var eb = new EmbedBuilder()
            {
                Color = Utility.BlueInfoEmbed,
                Title = $"{Utility.MusicalNote} Queue",
                Footer = Utility.RequestedBy(user),
            };
            
            // first show currently playing track
            eb.AddField(x =>
            {
                x.IsInline = false;
                x.Name = $"Now playing by {player.CurrentTrack.Author}";
                x.Value =
                    $"[{player.CurrentTrack.Length.ToString(@"mm\:ss")}] - **[{player.CurrentTrack.Title}]({player.CurrentTrack.Uri})**";
            });

            var queue = player.Queue;

            if (queue == null || queue.Count == 0) return eb.Build();


            int count = 0;
            foreach (var track in queue.Items)
            {
                eb.AddField(x =>
                {
                    x.IsInline = false;
                    x.Name = $"#{count+1} by {track.Author}";
                    x.Value =
                        $"[{track.Length.ToString(@"mm\:ss")}] - **[{track.Title}]({track.Uri})**";
                });

                count++;
                if (count >= 10) break;
            }

            // get total length
            TimeSpan span = new TimeSpan();

            foreach (var track in queue.Items)
            {
                span = span.Add(track.Length);
            }
            
            // also add currently playing song
            span = span.Add(player.CurrentTrack.Length.Subtract(player.Position));

            eb.AddField(x =>
            {
                x.IsInline = false;
                x.Name = $"{queue.Count} songs in queue";
                x.Value = $"[{span.ToString(@"hh\:mm\:ss")}] total playtime";
            });

            return eb.Build();

        }

        public async Task<Embed> SkipAsync(ulong guildId, ulong userId)
        {
            var player = _lavaNode.GetPlayer(guildId);
            if(player?.CurrentTrack == null) return Utility.ResultFeedback(
                Utility.BlueInfoEmbed,
                Utility.MusicalNote,
                "Not playing anything currently.").Build();

            if (player.Queue.Count == 0)
            {
                player.Stop();
                return Utility.ResultFeedback(
                    Utility.BlueInfoEmbed,
                    Utility.MusicalNote,
                    "The Queue is empty. Player has been stopped.")
                    .Build();
            }

            using (var soraContext = new SoraContext())
            {
                var guildDb = Utility.GetOrCreateGuild(guildId, soraContext);
                if (!guildDb.NeedVotes)
                {
                    var track = player.Skip();

                    return Utility.ResultFeedback(
                        Utility.BlueInfoEmbed,
                        Utility.MusicalNote,
                        $"Now playing: {track.Title}")
                        .WithUrl(track.Uri.ToString()).Build();
                }
            }

            var users = (await player.VoiceChannel.GetUsersAsync().FlattenAsync()).Count(x => !x.IsBot);
            if (!_voteSkip.ContainsKey(guildId))
                _voteSkip.TryAdd(guildId, (player.CurrentTrack, new List<ulong>()));
            _voteSkip.TryGetValue(guildId, out var skipInfo);
            
            if(!skipInfo.votes.Contains(userId)) skipInfo.votes.Add(userId);
            var perc = (int) Math.Round((100.0 * skipInfo.votes.Count) / users);
            if(perc <= 50) return Utility.ResultFeedback(
                Utility.BlueInfoEmbed,
                Utility.SuccessLevelEmoji[3],
                "More votes needed.").Build();
            _voteSkip.TryUpdate(guildId, skipInfo, skipInfo);
            var temp = player.CurrentTrack;
            player.Skip();
            return Utility.ResultFeedback(
                    Utility.BlueInfoEmbed,
                    Utility.MusicalNote,
                    $"Skipped: {temp.Title}")
                .WithUrl(temp.Uri.ToString()).Build();
        }

        public string Volume(ulong guildId, int vol)
        {
            var player = _lavaNode.GetPlayer(guildId);
            if (player == null) return "Not playing anything currently.";

            try
            {
                if (vol < 1)
                    vol = 1;
                else if (vol > 100)
                    vol = 100;
                player.Volume(vol);
                return $"Volume has been set to {vol}";
            }
            catch (ArgumentException e)
            {
                return e.Message;
            }
        }

        private async Task NodeOnException(LavaPlayer player, LavaTrack track, string arg3)
        {
            player.Dequeue();
            player.Enqueue(track);
            await player.TextChannel.SendMessageAsync(
                "",embed:Utility.ResultFeedback(
                        Utility.BlueInfoEmbed,
                        Utility.MusicalNote,
                        $"Track {track.Title} threw an exception. Track has been requeued.")
                    .WithDescription(string.IsNullOrWhiteSpace(arg3) ? "Unknown Exception" : arg3)
                    .Build());
        }
        
        private async Task NodeOnUpdated(LavaPlayer player, LavaTrack track, TimeSpan arg3)
        {
            // TODO internal counter for more accurate measurement of time passed.
        }

        private async Task NodeOnFinished(LavaPlayer player, LavaTrack track, TrackReason reason)
        {
            if (player == null)
                return;
            if (reason != TrackReason.Finished || reason != TrackReason.LoadFailed)
                return;
            
            // player.Remove(track);
            
            var nextTrack = player.Queue.Count == 0 ? null : player.Queue.Dequeue();
            if (nextTrack == null)
            {
                await _lavaNode.LeaveAsync(player.Guild.Id);
                await player.TextChannel.SendMessageAsync("", embed:Utility.ResultFeedback(
                        Utility.BlueInfoEmbed,
                        Utility.MusicalNote,
                        "Queue Completed!")
                    .Build());
                return;
            }

            player.Play(nextTrack);
            await player.TextChannel.SendMessageAsync("", embed:Utility.ResultFeedback(
                    Utility.BlueInfoEmbed,
                    Utility.MusicalNote,
                    $"Now Playing: {nextTrack.Title}")
                .WithUrl(nextTrack.Uri.ToString())
                .Build());
        }

        private async Task NodeOnStuck(LavaPlayer player, LavaTrack track, long arg3)
        {
            player.Dequeue();
            player.Enqueue(track);
            await player.TextChannel.SendMessageAsync(
                "", embed:Utility.ResultFeedback(
                        Utility.BlueInfoEmbed,
                        Utility.MusicalNote,
                        $"Track {track.Title} got stuck: {arg3}. Track has been requeued.")
                    .WithUrl(track.Uri.ToString())
                    .Build());
        }
    }
}