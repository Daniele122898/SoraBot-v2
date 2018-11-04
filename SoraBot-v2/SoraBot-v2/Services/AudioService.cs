using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using SoraBot_v2.Data;
using Victoria;
using Victoria.Objects;
using Victoria.Objects.Enums;

namespace SoraBot_v2.Services
{
    public class AudioService
    {

        private LavaNode _lavaNode;
        private readonly ConcurrentDictionary<ulong, (LavaTrack track, List<ulong> votes)> _voteSkip;
        
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

        public async Task<(LavaTrack track, bool enqued)> PlayAsync(ulong guildId, string query)
        {
            // if url get that otherwise search yt
            var search = Uri.IsWellFormedUriString(query, UriKind.RelativeOrAbsolute)
                ? await _lavaNode.GetTracksAsync(query)
                : await _lavaNode.SearchYouTubeAsync(query);

            // get first track
            var track = search.Tracks.FirstOrDefault();
            var player = _lavaNode.GetPlayer(guildId);
            if (player.CurrentTrack != null)
            {
                player.Enqueue(track);
                return (track, true);
            }
            
            player.Play(track);
            return (track, false);
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
            

            for (int i = 0; i < (player.Queue.Count <10 ? queue.Count : 10); i++)
            {
                var track = queue.ElementAt(i);
                eb.AddField(x =>
                {
                    x.IsInline = false;
                    x.Name = $"#{i+1} by {track.Author}";
                    x.Value =
                        $"[{track.Length.ToString(@"mm\:ss")}] - **[{track.Title}]({track.Uri})**";
                });
            }
            
            // get total length
            TimeSpan span = new TimeSpan();

            foreach (var track in queue)
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
            
            

            using (var soraContext = new SoraContext())
            {
                var guildDb = Utility.GetOrCreateGuild(guildId, soraContext);
                if (!guildDb.NeedVotes)
                {
                    var track = player.CurrentTrack;
                    player.Skip();
                    return Utility.ResultFeedback(
                        Utility.BlueInfoEmbed,
                        Utility.MusicalNote,
                        $"Skipped: {track.Title}")
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
            player.Dequeue(track);
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
            player.Dequeue(track);
            var queue = player.Queue;
            var nextTrack = queue.Count == 0 ? null : queue.First?.Value;
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
            player.Dequeue(track);
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