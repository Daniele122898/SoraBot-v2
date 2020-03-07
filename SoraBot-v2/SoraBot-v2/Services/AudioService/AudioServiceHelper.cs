using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using SoraBot_v2.Data.Entities.SubEntities;
using Victoria;
using Victoria.Entities;
using SearchResult = Victoria.Entities.SearchResult;

namespace SoraBot_v2.Services
{
    public partial class AudioService
    {
        public bool CheckSameVoiceChannel(ulong guildId, ulong? voiceId)
        {
            if (voiceId == null || voiceId == 0) return false;
            var player = _lavaSocketClient.GetPlayer(guildId);
            if (player == null) return false;
            return player.VoiceChannel?.Id == voiceId;
        }

        public bool PlayerIsntConnectedInGuild(ulong guildId)
        {
            var player = _lavaSocketClient.GetPlayer(guildId);
            if (player == null) return true;
            return false;
        }
        
        private async Task<LavaTrack> RepeatTrackPlay(string uri)
        {
            var search = await _lavaRestClient.SearchTracksAsync(uri);
            return search.Tracks.FirstOrDefault();
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
                    // ignored
                }
            }         
        }
        
        private async Task<(LavaTrack track, string reason)> GetSongFromSelect(SocketCommandContext context, SearchResult search)
        {
            // now lets build the embed to ask the user what to use
            EmbedBuilder eb = new EmbedBuilder
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
        
        public async Task<bool> PlayerExistsAndConnected(ulong guildId)
        {
            // get player
            var player = _lavaSocketClient.GetPlayer(guildId);
            var vc = _client.GetGuild(guildId).CurrentUser.VoiceChannel;
            if (player != null)
            {
                // the player exists check if we are connected to the VC in d.net as well
                if (vc?.Id == player.VoiceChannel?.Id)
                    return true;
                
                // in d.net we are not connected so remove the player.
                _options.TryRemove(guildId, out _);
                await _lavaSocketClient.DisconnectAsync(player.VoiceChannel);
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
            var player = _lavaSocketClient.GetPlayer(guild.Id);
            if (player == null) return;
            
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
                await _lavaSocketClient.DisconnectAsync(player.VoiceChannel);
                return;
            }
            
            // lastly check if the channel is an AFK channel and leave as well
            if (guild.AFKChannel.Id == ourChannel.Id)
            {
                _options.TryRemove(guild.Id, out _);
                await _lavaSocketClient.DisconnectAsync(player.VoiceChannel);
            }
        }
        
        // public async Task ClientOnDisconnected(Exception arg)
        // {
        //     //Make sure this shit is in a background thread.
        //     Task.Run(async () =>
        //     {
        //         Console.WriteLine("RE-CONFIGURING MUSIC STUFF");
        //         
        //         async Task LeavePlayer(ulong guildId, IVoiceChannel channel)
        //         {
        //             _options.TryRemove(guildId, out _);
        //             await _lavaSocketClient.DisconnectAsync(channel);
        //         }
        //         
        //         async Task ForceLeave(ulong guildId)
        //         {
        //             _options.TryRemove(guildId, out _);
        //             await _client.GetGuild(guildId).CurrentUser.VoiceChannel.DisconnectAsync();
        //         }
        //
        //         int tries = 0;
        //         while (_client.ConnectionState != ConnectionState.Connected)
        //         {
        //             await Task.Delay(3000);
        //             tries++;
        //             // only try this a couple times otherwise give up since the service is probably getting restarted
        //             if (tries >= 3)
        //             {
        //                 Console.WriteLine("FAILED RECONNECTION IN MUSIC RESUME. ABORTING");
        //                 return;
        //             }
        //         }
        //         
        //         Console.WriteLine("RESETTING NEEDED MUSIC STUFF");
        //         
        //         // now lets check all the guilds Sora is in a VoiceChannel.
        //         var VCs = _client.Guilds.SelectMany(x => x.VoiceChannels.Where(y => y.Users.Any(z => z.Id == _soraId)));
        //         // now lets do some checks for these VCs
        //         foreach (var vc in VCs)
        //         {
        //             var player = _lavaSocketClient.GetPlayer(vc.Guild.Id);
        //             // check if we are alone
        //             if (vc.Users.Count(x => !x.IsBot) == 0)
        //             {
        //                 // we are alone
        //                 // check if there is a player
        //                 if (player == null)
        //                 {
        //                     // there is no player so we force leave
        //                     await ForceLeave(vc.Guild.Id);
        //                 }
        //                 else
        //                 {
        //                     // there is a player so leave gracefully
        //                     await LeavePlayer(vc.Guild.Id, player.VoiceChannel);
        //                 }
        //             }
        //             // we're not alone
        //             // check if the player still exists tho. otherwise force leave
        //             if (player == null)
        //             {
        //                 // we're not alone but the player doesn't exist anymore
        //                 await ForceLeave(vc.Guild.Id);
        //             }
        //         }
        //         Console.WriteLine("DONE");
        //     });
        // }
    }
}