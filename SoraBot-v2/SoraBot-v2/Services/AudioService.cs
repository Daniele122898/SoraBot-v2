using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
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
            player.Queue.TryAdd(guildId, new LinkedList<LavaTrack>());
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
                ? await _lavaNode.GetTracksAsync(new Uri(query))
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

            var queue = player.Queue[guildId];

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

            eb.AddField(x =>
            {
                x.IsInline = false;
                x.Name = $"{queue.Count} songs in queue";
                x.Value = $"[{span.ToString(@"hh\:mm\:ss")}] total playtime";
            });

            return eb.Build();

        }

        private Task NodeOnException(LavaPlayer arg1, LavaTrack arg2, string arg3)
        {
            throw new System.NotImplementedException();
        }

        private Task NodeOnFinished(LavaPlayer arg1, LavaTrack arg2, TrackReason arg3)
        {
            throw new System.NotImplementedException();
        }

        private Task NodeOnStuck(LavaPlayer arg1, LavaTrack arg2, long arg3)
        {
            throw new System.NotImplementedException();
        }
    }
}