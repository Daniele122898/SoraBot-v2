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
using Discord.Addons.Interactive;
using SoraBot_v2.Data.Entities.SubEntities;
using Victoria.Entities;
using SearchResult = Victoria.Entities.SearchResult;

namespace SoraBot_v2.Services
{
    public partial class AudioService
    {

        private LavaSocketClient _lavaSocketClient;
        private LavaRestClient _lavaRestClient;
        private InteractiveService _interactive;
        private DiscordSocketClient _client;
        private ulong _soraId;
        private readonly ConcurrentDictionary<ulong, AudioOptions> _options = new ConcurrentDictionary<ulong, AudioOptions>();
        private ServerStats _serverStats = null;

        public AudioService(InteractiveService service, DiscordSocketClient client)
        {
            _interactive = service;
            _client = client;
        }

        public string ShuffleQueue(ulong guildId)
        {
            var player = _lavaSocketClient.GetPlayer(guildId);
            if (player?.CurrentTrack == null)
                return "Not playing anything currently.";
            player.Queue.Shuffle();
            return "Shuffled Queue :>";
        }

        public string ToggleRepeat(ulong guildId)
        {
            var player = _lavaSocketClient.GetPlayer(guildId);
            if (player?.CurrentTrack == null)
                return "Not playing anything currently.";
            if (!_options.TryGetValue(guildId, out var options))
                return "Something went wrong. Reconnect Sora please.";
            options.RepeatTrack = !options.RepeatTrack;
            return options.RepeatTrack ? "Song will now repeat." : "Repeat is now turned off.";
        }

        public string ClearQueue(ulong guildId)
        {
            var player = _lavaSocketClient.GetPlayer(guildId);
            if (player == null)
                return "Not playing anything currently.";
            if (player.Queue.Count == 0)
                return "Queue already empty";
            int songs = player.Queue.Count;
            player.Queue.Clear();
            return $"Cleared Queue. Removed {songs} Songs";
        }

        public EmbedBuilder PlayerStats(string avatarUrl, SocketUser requestor)
        {

            if (_serverStats == null)
            {
                return new EmbedBuilder()
                {
                    Color = Utility.RedFailiureEmbed,
                    Title = Utility.SuccessLevelEmoji[2] + " Stats are not yet available. Try later again."
                };
            }
            
            long FormatRamValue(long d)
            {
                while (d > 1000)
                {
                    d /= 1000;
                }
                return d;
            }

            string FormatRamUnit(long d)
            {
                var units = new string[] { "B", "KB", "MB", "GB", "TB", "PB" };
                var unitCount = 0;
                while (d > 1000)
                {
                    d /= 1000;
                    unitCount++;
                }
                return units[unitCount];
            }
            
            EmbedBuilder eb = new EmbedBuilder()
            {
                Color = Utility.BlueInfoEmbed,
                Title = $"{Utility.SuccessLevelEmoji[3]} LavaLink Stats",
                Description = "These stats are Global for LavaLink",
                ThumbnailUrl = avatarUrl,
                Footer = Utility.RequestedBy(requestor)
            };
            eb.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "Active -/ Total Players";
                x.Value = $"{_serverStats.PlayingPlayers} / {_serverStats.PlayerCount}";
            });
            eb.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "RAM usage";
                x.Value = $"{FormatRamValue(_serverStats.Memory.Used):f2} {FormatRamUnit(_serverStats.Memory.Used)} / " +
                          $"{FormatRamValue(_serverStats.Memory.Allocated):f2} {FormatRamUnit(_serverStats.Memory.Allocated)}";
            });
            eb.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "LavaLink CPU Count";
                x.Value = $"{_serverStats.Cpu.Cores}";
            });
            eb.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "LavaLink CPU Usage";
                x.Value = $"{(_serverStats.Cpu.LavalinkLoad*100):f2}%";
            });
    
            return eb;
        }

        public async Task<bool> ConnectAsync(ulong guildId, IGuildUser user, ITextChannel channel)
        {
            if (user.VoiceChannel == null)
            {
                await channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                        Utility.RedFailiureEmbed,
                        Utility.SuccessLevelEmoji[2],
                        "You aren't connected to any voice channels.")
                    .Build());
                return false;
            }
            
            // check if someone summoned me before
            if (_options.TryGetValue(guildId, out var options) && options.Summoner.Id != user.Id && await PlayerExistsAndConnected(guildId))
            {
                await channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                        Utility.RedFailiureEmbed,
                        Utility.SuccessLevelEmoji[2],
                        $"I can't join another Voice Channel until {options.Summoner.Username}#{options.Summoner.Discriminator} disconnects me. >.<")
                    .Build());
                return false;
            }

            await _lavaSocketClient.ConnectAsync(user.VoiceChannel, channel);
            _options.TryAdd(guildId, new AudioOptions()
            {
                Summoner = user,
                Voters = new HashSet<ulong>()
            });
            await channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                Utility.GreenSuccessEmbed,
                Utility.SuccessLevelEmoji[0],
                $"Connected to {user.VoiceChannel} and bound to {channel.Name}.")
                .Build());
            return true;
        }

        public async Task YoutubeOrSoundCloudSearch(SocketCommandContext context, string query, bool youtube)
        {
            var player = _lavaSocketClient.GetPlayer(context.Guild.Id);
            if (player == null)
            {
                await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                        Utility.RedFailiureEmbed,
                        Utility.SuccessLevelEmoji[2],
                        "Connect me to a Voice Channel first!")
                    .Build());
                return;
            }
            var search = youtube ? await _lavaRestClient.SearchYouTubeAsync(query) : await _lavaRestClient.SearchSoundcloudAsync(query);
            if (search.LoadType == LoadType.NoMatches || search.LoadType == LoadType.LoadFailed)
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
            
            bool queued = false;
            if (player.CurrentTrack != null)
            {
                player.Queue.Enqueue(result.track);
                queued = true;
            }
            else
                await player.PlayAsync(result.track);
            
            await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                    Utility.GreenSuccessEmbed,
                    Utility.MusicalNote,
                    $"{(queued ? "Enqueued" : "Playing")}: [{result.track.Length.ToString(@"mm\:ss")}] - **{result.track.Title}**")
                .WithUrl(result.track.Uri.ToString())
                .Build());
        }

        public async Task<(LavaTrack track, bool enqued, string name, int num)> PlayAsync(ulong guildId, string query)
        {
            var player = _lavaSocketClient.GetPlayer(guildId);

            if (player == null)
            {
                return (null, false, null, -1);
            }
            
            if (query.StartsWith("<") && query.EndsWith(">"))
                query = query.TrimStart('<').TrimEnd('>');
            // if url get that otherwise search yt
            bool isLink = Uri.IsWellFormedUriString(query, UriKind.RelativeOrAbsolute);
            
            var search = isLink
                ? await _lavaRestClient.SearchTracksAsync(query)
                : await _lavaRestClient.SearchYouTubeAsync(query);
            
            if (search.LoadType == LoadType.NoMatches 
                || search.LoadType == LoadType.LoadFailed
                || search.Tracks == null
                || !search.Tracks.Any())
            {
                return (null, false, null, 0);
            }

            if (!isLink || search.Tracks.Count() == 1)
            {
                // get first track
                var track = search.Tracks.FirstOrDefault();
                if (player.CurrentTrack != null)
                {
                    player.Queue.Enqueue(track);
                    return (track, true, null, 1);
                }
                await player.PlayAsync(track);
                return (track, false, null, 1);
            }
            
            // get playlist
            await loadPlaylist(_lavaSocketClient.GetPlayer(guildId), search.Tracks);
            return (null, false, search.PlaylistInfo.Name, search.Tracks.Count());
        }

        public async Task<string> DisconnectAsync(ulong guildId)
        {
            // remove options
            var player = _lavaSocketClient.GetPlayer(guildId);
            if (player == null)
                return "Not connected to any voice channels.";
            _options.TryRemove(guildId, out _);
            await _lavaSocketClient.DisconnectAsync(player.VoiceChannel);
            return "Disconnected.";
        }

        public async Task<string> Pause(ulong guildId)
        {
            var player = _lavaSocketClient.GetPlayer(guildId);
            if(player?.CurrentTrack == null) return "Not playing anything currently.";
            if (player.IsPaused) return "Player is already paused.";
            await player.PauseAsync();
            return $"Paused: {player.CurrentTrack.Title}";
        }

        public async Task<(string message, bool error)> ForceDisconnect(ulong guildId)
        {
            if (_lavaSocketClient.GetPlayer(guildId) != null)
            {
                return ("The player seems alright. Use normal disconnect!", true);
            }
            // the player is FACKED so lets actually force DC
            // check if we're in VC anyway and if so leave.
            var vc = _client.GetGuild(guildId).CurrentUser.VoiceChannel;

            if (vc == null)
            {
                return ("Sora is not connected to any Voice channel!", true);
            }
            // remove options if they are still there
            _options.TryRemove(guildId, out _);
            // forcefully disconnect Sora
            await vc.DisconnectAsync();
            return ("Forcefully disconnected Sora. You should be able to use him again. Gomen >.<", false);
        }

        public async Task<string> Resume(ulong guildId)
        {
            var player = _lavaSocketClient.GetPlayer(guildId);
            if(player?.CurrentTrack == null) return "Not playing anything currently.";
            if (!player.IsPaused) return "Player is not paused.";
            await player.PauseAsync();
            return $"Resumed: {player.CurrentTrack.Title}";
        }

        public Embed NowPlaying(ulong guildId)
        {
            var player = _lavaSocketClient.GetPlayer(guildId);
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
                                    player.CurrentTrack.Position.TotalSeconds;

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
                        $"[{player.CurrentTrack.Position.ToString(@"mm\:ss")}] {progress} [{player.CurrentTrack.Length.ToString(@"mm\:ss")}]";
                })
                .Build();    
        }

        public Embed DisplayQueue(ulong guildId, SocketUser user, IMessageChannel channel)
        {
            var player = _lavaSocketClient.GetPlayer(guildId);
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
            foreach (var queueItem in queue.Items)
            {
                var track = queueItem as LavaTrack;
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

            foreach (var queueItem in queue.Items)
            {
                var track = queueItem as LavaTrack;
                span = span.Add(track.Length);
            }
            
            // also add currently playing song
            span = span.Add(player.CurrentTrack.Length.Subtract(player.CurrentTrack.Position));

            eb.AddField(x =>
            {
                x.IsInline = false;
                x.Name = $"{queue.Count} songs in queue";
                x.Value = $"[{span.ToString(@"hh\:mm\:ss")}] total playtime";
            });

            return eb.Build();

        }

        public async Task<Embed> SkipAsync(ulong guildId, SocketGuildUser user)
        {
            var player = _lavaSocketClient.GetPlayer(guildId);
            if(player?.CurrentTrack == null) return Utility.ResultFeedback(
                Utility.BlueInfoEmbed,
                Utility.MusicalNote,
                "Not playing anything currently.").Build();

            using (var soraContext = new SoraContext())
            {
                var guildDb = Utility.GetOrCreateGuild(guildId, soraContext);
                if (!guildDb.NeedVotes)
                {
                    if (player.Queue.Count == 0)
                    {
                        await player.StopAsync();
                        return Utility.ResultFeedback(
                                Utility.BlueInfoEmbed,
                                Utility.MusicalNote,
                                "The Queue is empty. Player has been stopped.")
                            .Build();
                    }

                    await player.SkipAsync();
                    
                    var track = player.CurrentTrack;
                    
                    return Utility.ResultFeedback(
                        Utility.BlueInfoEmbed,
                        Utility.MusicalNote,
                        $"Now playing: {track.Title}")
                        .WithUrl(track.Uri.ToString()).Build();
                }
            }

            _options.TryGetValue(guildId, out var options);
            
            if(options == null)
                return Utility.ResultFeedback(
                        Utility.RedFailiureEmbed,
                        Utility.SuccessLevelEmoji[2],
                        "Something went terribly wrong. Reconnect Sora to the Voice Channel!")
                    .Build();
            
            if (options.Voters.Contains(user.Id))
                return Utility.ResultFeedback(
                        Utility.RedFailiureEmbed,
                        Utility.SuccessLevelEmoji[2],
                        "You've already voted. Please don't vote again.")
                    .Build();

            options.VotedTrack = player.CurrentTrack;
            options.Voters.Add(user.Id);
            
            var perc = (float)options.Voters.Count / user.VoiceChannel.Users.Count(x => !x.IsBot) * 100;

            if (perc < 51f)
                return Utility.ResultFeedback(
                        Utility.YellowWarningEmbed,
                        Utility.SuccessLevelEmoji[1],
                        "More votes needed to skip. It requires **more** than 50% of users in the Voice Channel.")
                    .Build();
            
            if (player.Queue.Count == 0)
            {
                await player.StopAsync();
                return Utility.ResultFeedback(
                        Utility.BlueInfoEmbed,
                        Utility.MusicalNote,
                        "The Queue is empty. Player has been stopped.")
                    .Build();
            }

            var next = await player.SkipAsync();
            RemoveVotes(guildId, options);

            return Utility.ResultFeedback(
                    Utility.BlueInfoEmbed,
                    Utility.MusicalNote,
                    $"Now playing: {next.Title}")
                .WithUrl(next.Uri.ToString()).Build();
        }

        public async Task<string> Volume(ulong guildId, ushort vol)
        {
            var player = _lavaSocketClient.GetPlayer(guildId);
            if (player == null || !player.IsPlaying) return "Not playing anything currently.";

            try
            {
                if (vol < 1)
                    vol = 1;
                else if (vol > 100)
                    vol = 100;
                await player.SetVolumeAsync(vol);
                return $"Volume has been set to {vol}";
            }
            catch (ArgumentException e)
            {
                return e.Message;
            }
        }
        
        
    }
}