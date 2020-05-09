using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using SoraBot.Common.Extensions.Modules;
using Victoria;
using Victoria.Enums;

namespace SoraBot.Bot.Modules.AudioModule
{
    
    [Name("Music")]
    [Summary("All commands around music playing :>")]
    public partial class AudioModule : SoraSocketCommandModule
    {
        private readonly LavaNode _node;

        public AudioModule(LavaNode node)
        {
            _node = node;
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
            
            if (player.PlayerState != PlayerState.Playing)
            {
                await ReplyFailureEmbed("I'm currently not playing anything");
                return;
            }

            try
            {
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