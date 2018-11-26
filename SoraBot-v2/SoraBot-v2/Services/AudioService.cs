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
using Humanizer;
using SoraBot_v2.Data.Entities.SubEntities;
using Victoria.Entities;
using Victoria.Entities.Enums;
using Victoria.Entities.Stats;

namespace SoraBot_v2.Services
{
    public class AudioService
    {

        private LavaNode _lavaNode;
        private InteractiveService _interactive;
        private DiscordSocketClient _client;
        private Server? _server = null;
        private ulong _soraId;
        private readonly ConcurrentDictionary<ulong, AudioOptions> _options = new ConcurrentDictionary<ulong, AudioOptions>();

        public AudioService(InteractiveService service, DiscordSocketClient client)
        {
            _interactive = service;
            _client = client;
        }

        public bool CheckSameVoiceChannel(ulong guildId, ulong? voiceId)
        {
            if (voiceId == null || voiceId == 0) return false;
            var player = _lavaNode.GetPlayer(guildId);
            if (player == null) return false;
            return player.VoiceChannel?.Id == voiceId;
        }
        
        public async Task ClientOnDisconnected(Exception arg)
        {
            //Make sure this shit is in a background thread.
            Task.Run(async () =>
            {
                Console.WriteLine("RE-CONFIGURING MUSIC STUFF");
                
                async Task LeavePlayer(ulong guildId)
                {
                    _options.TryRemove(guildId, out _);
                    await _lavaNode.DisconnectAsync(guildId);
                }
                
                async Task ForceLeave(ulong guildId)
                {
                    _options.TryRemove(guildId, out _);
                    await _client.GetGuild(guildId).CurrentUser.VoiceChannel.DisconnectAsync();
                }

                int tries = 0;
                while (_client.ConnectionState != ConnectionState.Connected)
                {
                    await Task.Delay(3000);
                    tries++;
                    // only try this a couple times otherwise give up since the service is probably getting restarted
                    if (tries >= 3)
                    {
                        Console.WriteLine("FAILED RECONNECTION IN MUSIC RESUME. ABORTING");
                        return;
                    }
                }
                
                Console.WriteLine("RESETTING NEEDED MUSIC STUFF");
                
                // now lets check all the guilds Sora is in a VoiceChannel.
                var VCs = _client.Guilds.SelectMany(x => x.VoiceChannels.Where(y => y.Users.Any(z => z.Id == _soraId)));
                // now lets do some checks for these VCs
                foreach (var vc in VCs)
                {
                    var player = _lavaNode.GetPlayer(vc.Guild.Id);
                    // check if we are alone
                    if (vc.Users.Count(x => !x.IsBot) == 0)
                    {
                        // we are alone
                        // check if there is a player
                        if (player == null)
                        {
                            // there is no player so we force leave
                            await ForceLeave(vc.Guild.Id);
                        }
                        else
                        {
                            // there is a player so leave gracefully
                            await LeavePlayer(vc.Guild.Id);
                        }
                    }
                    // we're not alone
                    // check if the player still exists tho. otherwise force leave
                    if (player == null)
                    {
                        // we're not alone but the player doesn't exist anymore
                        await ForceLeave(vc.Guild.Id);
                    }
                }
                Console.WriteLine("DONE");
            });
        }

        public async Task<bool> PlayerExistsAndConnected(ulong guildId)
        {
            // get player
            var player = _lavaNode.GetPlayer(guildId);
            var vc = _client.GetGuild(guildId).CurrentUser.VoiceChannel;
            if (player != null)
            {
                // the player exists check if we are connected to the VC in d.net as well
                if (vc?.Id == player.VoiceChannel?.Id)
                    return true;
                
                // in d.net we are not connected so remove the player.
                _options.TryRemove(guildId, out _);
                await _lavaNode.DisconnectAsync(guildId);
                return false;
            }
            // player is null lets check if he's connected tho.
            _options.TryRemove(guildId, out _);
            // player is null and vc is null so its disconnected
            if (vc == null)
                return false;
            
            // we are connected so lets force disconnect
            await vc.DisconnectAsync();
            return false;
        }
        
        public async Task ClientOnUserVoiceStateUpdated(SocketUser user, SocketVoiceState oldState, SocketVoiceState newState)
        {
            var guild = newState.VoiceChannel?.Guild ?? oldState.VoiceChannel?.Guild;
            if (guild == null) return;
            if (_lavaNode.GetPlayer(guild.Id) == null) return;
            
            // now we know its a voice channel that we actually care about. So lets do shit
            // find the voice channel in which Sora is in
            SocketVoiceChannel ourChannel = null;
            if (oldState.VoiceChannel.Users.FirstOrDefault(x => x.Id == _soraId) != null)
                ourChannel = oldState.VoiceChannel;
            else
                ourChannel = newState.VoiceChannel;
            
            if(ourChannel == null) return;
            
            var userCount = ourChannel.Users.Count(x => !x.IsBot);
            // there are no real users -> leave
            if (userCount == 0)
            {
                _options.TryRemove(guild.Id, out _);
                await _lavaNode.DisconnectAsync(guild.Id);
                return;
            }
            
            // lastly check if the channel is an AFK channel and leave as well
            if (guild.AFKChannel.Id == ourChannel.Id)
            {
                _options.TryRemove(guild.Id, out _);
                await _lavaNode.DisconnectAsync(guild.Id);
            }
        }
        
        public void Initialize(LavaNode node, ulong soraId)
        {
            _soraId = soraId;
            _lavaNode = node;
            // lavanode events
            node.TrackStuck = NodeOnStuck;
            node.TrackFinished = NodeOnFinished;
            node.TrackException = NodeOnException;
            node.StatsUpdated = StatsUpdated;
            //node.PlayerUpdated = PlayerUpdated;
        }

        private Task PlayerUpdated(LavaPlayer player, LavaTrack track, TimeSpan time)
        {
            Console.WriteLine($"Update: {player.VoiceChannel.GuildId}: {track.Title} - {time.TotalSeconds}");
            return Task.CompletedTask;
        }

        private Task StatsUpdated(Server server)
        {
            Console.WriteLine("GOT STATS");
            _server = server;
            return Task.CompletedTask;
        }

        public string ShuffleQueue(ulong guildId)
        {
            var player = _lavaNode.GetPlayer(guildId);
            if (player?.CurrentTrack == null)
                return "Not playing anything currently.";
            player.Queue.Shuffle();
            return "Shuffled Queue :>";
        }

        public string ToggleRepeat(ulong guildId)
        {
            var player = _lavaNode.GetPlayer(guildId);
            if (player?.CurrentTrack == null)
                return "Not playing anything currently.";
            if (!_options.TryGetValue(guildId, out var options))
                return "Something went wrong. Reconnect Sora please.";
            options.RepeatTrack = !options.RepeatTrack;
            return options.RepeatTrack ? "Song will now repeat." : "Repeat is now turned off.";
        }

        public string ClearQueue(ulong guildId)
        {
            var player = _lavaNode.GetPlayer(guildId);
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

            if (_server == null)
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
                x.Value = $"{_server.Value.PlayingPlayers} / {_server.Value.Players}";
            });
            eb.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "RAM usage";
                x.Value = $"{FormatRamValue(_server.Value.Memory.Used):f2} {FormatRamUnit(_server.Value.Memory.Used)} / {FormatRamValue(_server.Value.Memory.Allocated):f2} {FormatRamUnit(_server.Value.Memory.Allocated)}";
            });
            eb.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "LavaLink CPU Count";
                x.Value = $"{_server.Value.CPU.Cores}";
            });
            eb.AddField(x =>
            {
                x.IsInline = true;
                x.Name = "LavaLink CPU Usage";
                x.Value = $"{(_server.Value.CPU.LavalinkLoad*100):f2}%";
            });
    
            return eb;
        }

        public async Task ConnectAsync(ulong guildId, IGuildUser user, IMessageChannel channel)
        {
            if (user.VoiceChannel == null)
            {
                await channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                        Utility.RedFailiureEmbed,
                        Utility.SuccessLevelEmoji[2],
                        "You aren't connected to any voice channels.")
                    .Build());
                return;
            }
            
            // check if someone summoned me before
            if (_options.TryGetValue(guildId, out var options) && options.Summoner.Id != user.Id && await PlayerExistsAndConnected(guildId))
            {
                await channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                        Utility.RedFailiureEmbed,
                        Utility.SuccessLevelEmoji[2],
                        $"I can't join another Voice Channel until {options.Summoner.Username}#{options.Summoner.Discriminator} disconnects me. >.<")
                    .Build());
                return;
            }

            await _lavaNode.ConnectAsync(user.VoiceChannel, channel);
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
        }

        private async Task loadPlaylist(LavaPlayer player, IEnumerable<LavaTrack> tracks)
        {
            // load in playlist
            foreach (LavaTrack track in tracks)
            {
                try
                {
                    if (player.CurrentTrack != null)
                        player.Queue.Enqueue(track);
                    else
                        await player.PlayAsync(track);
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
            var player = _lavaNode.GetPlayer(context.Guild.Id);
            if (player == null)
            {
                await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                        Utility.RedFailiureEmbed,
                        Utility.SuccessLevelEmoji[2],
                        "Connect me to a Voice Channel first!")
                    .Build());
                return;
            }
            var search = youtube ? await _lavaNode.SearchYouTubeAsync(query) : await _lavaNode.SearchSoundcloudAsync(query);
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

        private async Task<LavaTrack> RepeatTrackPlay(string uri)
        {
            var search = await _lavaNode.GetTracksAsync(uri);
            return search.Tracks.FirstOrDefault();
        }

        public async Task<(LavaTrack track, bool enqued, string name, int num)> PlayAsync(ulong guildId, string query)
        {
            var player = _lavaNode.GetPlayer(guildId);

            if (player == null)
            {
                return (null, false, null, -1);
            }
            
            if (query.StartsWith("<") && query.EndsWith(">"))
                query = query.TrimStart('<').TrimEnd('>');
            // if url get that otherwise search yt
            bool isLink = Uri.IsWellFormedUriString(query, UriKind.RelativeOrAbsolute);
            
            var search = isLink
                ? await _lavaNode.GetTracksAsync(query)
                : await _lavaNode.SearchYouTubeAsync(query);
            
            if (search.LoadResultType == LoadResultType.NoMatches 
                || search.LoadResultType == LoadResultType.LoadFailed
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
            await loadPlaylist(_lavaNode.GetPlayer(guildId), search.Tracks);
            return (null, false, search.PlaylistInfo.Name, search.Tracks.Count());
        }

        public async Task<string> DisconnectAsync(ulong guildId)
        {
            // remove options
            if (_lavaNode.GetPlayer(guildId) == null)
                return "Not connected to any voice channels.";
            _options.TryRemove(guildId, out _);
            await _lavaNode.DisconnectAsync(guildId);
            return "Disconnected.";
        }

        public async Task<string> Pause(ulong guildId)
        {
            var player = _lavaNode.GetPlayer(guildId);
            if(player?.CurrentTrack == null) return "Not playing anything currently.";
            await player.PauseAsync();
            return $"Paused: {player.CurrentTrack.Title}";
        }

        public async Task<(string message, bool error)> ForceDisconnect(ulong guildId)
        {
            if (_lavaNode.GetPlayer(guildId) != null)
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
            var player = _lavaNode.GetPlayer(guildId);
            if(player?.CurrentTrack == null) return "Not playing anything currently.";
            await player.ResumeAsync();
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
                    if (player.Queue.Count == 0)
                    {
                        await player.StopAsync();
                        return Utility.ResultFeedback(
                                Utility.BlueInfoEmbed,
                                Utility.MusicalNote,
                                "The Queue is empty. Player has been stopped.")
                            .Build();
                    }
                    
                    var track = await player.SkipAsync();

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

        private void RemoveVotes(ulong guildId, AudioOptions options = null)
        {
            if (options == null)
            {
                if (!_options.TryGetValue(guildId, out options))
                    return;
            }
            options.VotedTrack = null;
            options.Voters.Clear();
        }

        public async Task<string> Volume(ulong guildId, ushort vol)
        {
            var player = _lavaNode.GetPlayer(guildId);
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

        private async Task NodeOnException(LavaPlayer player, LavaTrack track, string arg3)
        {
            RemoveVotes(player.VoiceChannel.GuildId);
            player.Queue.Remove(track);
            await player.TextChannel.SendMessageAsync(
                "",embed:Utility.ResultFeedback(
                        Utility.BlueInfoEmbed,
                        Utility.MusicalNote,
                        $"Track {track.Title} threw an exception. Track has been removed.")
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
            if (reason != TrackReason.Finished)
                return;
            
            // player.Remove(track);

            ulong guildId = player.VoiceChannel.GuildId;

            _options.TryGetValue(guildId, out var options);

            LavaTrack nextTrack = null;

            if (options != null && options.RepeatTrack)
                nextTrack = await RepeatTrackPlay(track.Uri.ToString());
            else
            {
                nextTrack = player.Queue.Count == 0 ? null : player.Queue.Dequeue();
            }            
            
            RemoveVotes(guildId);
            if (nextTrack == null)
            {
                await player.TextChannel.SendMessageAsync("", embed:Utility.ResultFeedback(
                        Utility.BlueInfoEmbed,
                        Utility.MusicalNote,
                        "Queue Completed!")
                    .Build());
                return;
            }

            await player.PlayAsync(nextTrack);
            await player.TextChannel.SendMessageAsync("", embed:Utility.ResultFeedback(
                    Utility.BlueInfoEmbed,
                    Utility.MusicalNote,
                    $"Now Playing: {nextTrack.Title}")
                .WithUrl(nextTrack.Uri.ToString())
                .Build());
        }

        private async Task NodeOnStuck(LavaPlayer player, LavaTrack lavaTrack, long arg3)
        {
            RemoveVotes(player.VoiceChannel.GuildId);
            player.Queue.Remove(lavaTrack);
            await player.TextChannel.SendMessageAsync(
                "", embed:Utility.ResultFeedback(
                        Utility.BlueInfoEmbed,
                        Utility.MusicalNote,
                        $"Track {lavaTrack.Title} got stuck: {arg3}. Track has been removed.")
                    .WithUrl(lavaTrack.Uri.ToString())
                    .Build());
        }
    }
}