using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using SoraBot_v2.Data;
using SoraBot_v2.Services;

namespace SoraBot_v2.Module
{
    [Name("Music")]
    public class AudioModule : ModuleBase<SocketCommandContext>
    {
        private AudioService _audio;

        public AudioModule(AudioService service)
        {
            _audio = service;
        }

        private ulong? GetVoiceChannelId(SocketUser user)
            => (user as SocketGuildUser)?.VoiceChannel?.Id;

        [Command("join"), Summary("Makes Sora join the Voice Channel you're in.")]
        public Task Join() 
            => _audio.ConnectAsync(Context.Guild.Id, ((IGuildUser)Context.User), Context.Channel as ITextChannel);

        [Command("leave"), Alias("stop"), Summary("Makes sora leave your Voice Channel. This also empties the queue and resets all options.")]
        public async Task StopAsync()
        {
            if (await _audio.PlayerExistsAndConnected(Context.Guild.Id) &&  !_audio.CheckSameVoiceChannel(Context.Guild.Id, GetVoiceChannelId(Context.User)))
            {
                await ReplyAsync("", embed: Utility.ResultFeedback(
                        Utility.RedFailiureEmbed,
                        Utility.SuccessLevelEmoji[2],
                        "You must be in the same Voice Channel as me!")
                    .Build());
                return;
            }
            await ReplyAsync("", embed: Utility.ResultFeedback(
                    Utility.BlueInfoEmbed,
                    Utility.SuccessLevelEmoji[3],
                    await _audio.DisconnectAsync(Context.Guild.Id))
                .Build());
        }

        [Command("forcedc"), Alias("forcedisconnect", "forceleave", "fdc", "fleave"),
         Summary("If Sora gets stuck in a Voice Channel you can use this command to unfuck the situation.")]
        public async Task ForceDc()
        {
            var res = await _audio.ForceDisconnect(Context.Guild.Id);
            await ReplyAsync("", embed: Utility.ResultFeedback(
            res.error ? Utility.RedFailiureEmbed : Utility.GreenSuccessEmbed,
                res.error ? Utility.SuccessLevelEmoji[2] : Utility.SuccessLevelEmoji[0],
                res.message)
                .Build());
        }


        [Command("sc", RunMode = RunMode.Async), Alias("soundcloud"), Summary("Searches Soundcloud for the track and gives you a list of found items to choose from.")]
        public async Task ScSearch([Remainder] string query)
        {
            if (!_audio.CheckSameVoiceChannel(Context.Guild.Id, GetVoiceChannelId(Context.User)))
            {
                await ReplyAsync("", embed: Utility.ResultFeedback(
                        Utility.RedFailiureEmbed,
                        Utility.SuccessLevelEmoji[2],
                        "You must be in the same Voice Channel as me!")
                    .Build());
                return;
            }
            await _audio.YoutubeOrSoundCloudSearch(Context, query, false);
        }

        [Command("yt", RunMode = RunMode.Async), Alias("youtube"), Summary("Searches Youtube for the track and gives you a list of found items to choose from.")]
        public async Task YtSearch([Remainder] string query)
        {
            if (!_audio.CheckSameVoiceChannel(Context.Guild.Id, GetVoiceChannelId(Context.User)))
            {
                await ReplyAsync("", embed: Utility.ResultFeedback(
                        Utility.RedFailiureEmbed,
                        Utility.SuccessLevelEmoji[2],
                        "You must be in the same Voice Channel as me!")
                    .Build());
                return;
            }
            await _audio.YoutubeOrSoundCloudSearch(Context, query, true);
        }

        [Command("play", RunMode = RunMode.Async), Alias("add"), Summary("If you add a link it will add that song or playlist. You can also add a name of a song and it will search youtube and take the first result.")]
        public async Task PlayAsync([Remainder] string query)
        {
            // if we're not connected in this guild we should connect and then play the music.
            if (_audio.PlayerIsntConnectedInGuild(Context.Guild.Id))
            {
                //no player exists in this guild. connect him
                bool success = await _audio.ConnectAsync(Context.Guild.Id, ((IGuildUser)Context.User), Context.Channel as ITextChannel);
                if (!success) return;
            }
            else if (!_audio.CheckSameVoiceChannel(Context.Guild.Id, GetVoiceChannelId(Context.User)))
            {
                await ReplyAsync("", embed: Utility.ResultFeedback(
                        Utility.RedFailiureEmbed,
                        Utility.SuccessLevelEmoji[2],
                        "You must be in the same Voice Channel as me!")
                    .Build());
                return;
            }
            
            var info = await _audio.PlayAsync(Context.Guild.Id, query);
            if (info.num == -1)
            {
                await ReplyAsync("", embed: Utility.ResultFeedback(
                        Utility.RedFailiureEmbed,
                        Utility.SuccessLevelEmoji[2],
                        "Connect me to a Voice Channel first!")
                    .Build());
                return;
            }
            if (info.num == 0)
            {
                await ReplyAsync("", embed: Utility.ResultFeedback(
                        Utility.RedFailiureEmbed,
                        Utility.SuccessLevelEmoji[2],
                        "Couldn't find anything.")
                    .Build());
                return;
            }
            if (string.IsNullOrWhiteSpace(info.name))
            {
                await ReplyAsync("", embed: Utility.ResultFeedback(
                        Utility.GreenSuccessEmbed,
                        Utility.MusicalNote,
                        $"{(info.enqued ? "Enqueued" : "Playing")}: [{info.track.Length.ToString(@"mm\:ss")}] - **{info.track.Title}**")
                    .WithUrl(info.track.Uri.ToString())
                    .Build());
                return;
            }
            await ReplyAsync("", embed: Utility.ResultFeedback(
                    Utility.GreenSuccessEmbed,
                    Utility.MusicalNote,
                    $"Loaded {info.name} with {info.num} Songs.")
                .Build());
        }

        [Command("msys"), Alias("musicsys"), Summary("Some stats for Lavalink")]
        public Task Msys()
            => ReplyAsync("",
                embed: _audio.PlayerStats(Context.Client.CurrentUser.GetAvatarUrl(), Context.User).Build());

        [Command("pause"), Summary("Pauses music playback")]
        public async Task Pause()
        {
            if (!_audio.CheckSameVoiceChannel(Context.Guild.Id, GetVoiceChannelId(Context.User)))
            {
                await ReplyAsync("", embed: Utility.ResultFeedback(
                        Utility.RedFailiureEmbed,
                        Utility.SuccessLevelEmoji[2],
                        "You must be in the same Voice Channel as me!")
                    .Build());
                return;
            }
            await ReplyAsync("", embed: Utility.ResultFeedback(
                    Utility.BlueInfoEmbed,
                    Utility.MusicalNote,
                    await _audio.Pause(Context.Guild.Id))
                .Build());
        }

        [Command("resume"), Summary("Resumes music playback")]
        public async Task Resume()
        {
            if (!_audio.CheckSameVoiceChannel(Context.Guild.Id, GetVoiceChannelId(Context.User)))
            {
                await ReplyAsync("", embed: Utility.ResultFeedback(
                        Utility.RedFailiureEmbed,
                        Utility.SuccessLevelEmoji[2],
                        "You must be in the same Voice Channel as me!")
                    .Build());
                return;
            }
            await ReplyAsync("", embed: Utility.ResultFeedback(
                    Utility.BlueInfoEmbed,
                    Utility.MusicalNote,
                    await _audio.Resume(Context.Guild.Id))
                .Build());
        }

        [Command("queue"), Alias("list"), Summary("Shows the current Queue.")]
        public Task Queue()
            => ReplyAsync("", embed: _audio.DisplayQueue(Context.Guild.Id, Context.User, Context.Channel));

        [Command("repeat"), Alias("togglerepeat", "toggle repeat", "repeat song"),
         Summary("Repeats the current song once its finished.")]
        public async Task ToggleRepeat()
        {
            if (!_audio.CheckSameVoiceChannel(Context.Guild.Id, GetVoiceChannelId(Context.User)))
            {
                await ReplyAsync("", embed: Utility.ResultFeedback(
                        Utility.RedFailiureEmbed,
                        Utility.SuccessLevelEmoji[2],
                        "You must be in the same Voice Channel as me!")
                    .Build());
                return;
            }     
            await ReplyAsync("", embed: Utility.ResultFeedback(
                    Utility.BlueInfoEmbed,
                    Utility.MusicalNote,
                    _audio.ToggleRepeat(Context.Guild.Id))
                .Build());
        }

        [Command("shuffle"), Alias("shufflequeue", "shufflelist"),
         Summary("Shuffles the entire queue. Cannot be undone.")]
        public async Task ShuffleQueue()
        {
            if (!_audio.CheckSameVoiceChannel(Context.Guild.Id, GetVoiceChannelId(Context.User)))
            {
                await ReplyAsync("", embed: Utility.ResultFeedback(
                        Utility.RedFailiureEmbed,
                        Utility.SuccessLevelEmoji[2],
                        "You must be in the same Voice Channel as me!")
                    .Build());
                return;
            }
            await ReplyAsync("", embed: Utility.ResultFeedback(
                    Utility.BlueInfoEmbed,
                    Utility.MusicalNote,
                    _audio.ShuffleQueue(Context.Guild.Id))
                .Build());
        }

        [Command("clear"), Alias("clearqueue"), Summary("Clears the entire queue. Cannot be undone.")]
        public async Task ClearQueue()
        {
            if (!_audio.CheckSameVoiceChannel(Context.Guild.Id, GetVoiceChannelId(Context.User)))
            {
                await ReplyAsync("", embed: Utility.ResultFeedback(
                        Utility.RedFailiureEmbed,
                        Utility.SuccessLevelEmoji[2],
                        "You must be in the same Voice Channel as me!")
                    .Build());
                return;
            }
            await ReplyAsync("", embed: Utility.ResultFeedback(
                    Utility.BlueInfoEmbed,
                    Utility.MusicalNote,
                    _audio.ClearQueue(Context.Guild.Id))
                .Build());
        }

        [Command("skip"), Alias("next"), Summary("Skips the current song.")]
        public async Task SkipAsync()
        {
            if (!_audio.CheckSameVoiceChannel(Context.Guild.Id, GetVoiceChannelId(Context.User)))
            {
                await ReplyAsync("", embed: Utility.ResultFeedback(
                        Utility.RedFailiureEmbed,
                        Utility.SuccessLevelEmoji[2],
                        "You must be in the same Voice Channel as me!")
                    .Build());
                return;
            }
            await ReplyAsync("", embed: await _audio.SkipAsync(Context.Guild.Id, (SocketGuildUser)Context.User));
        }

        [Command("voteskip"), Summary("Toggles Voteskipping ON or OFF")]
        public async Task ToggleVoteSkip()
        {
            var invoker = (SocketGuildUser)Context.User;
            if (!invoker.GuildPermissions.Has(GuildPermission.Administrator) && !Utility.IsSoraAdmin(invoker))
            {
                await ReplyAsync("", embed: Utility.ResultFeedback(
                    Utility.RedFailiureEmbed, 
                    Utility.SuccessLevelEmoji[2], 
                    $"You need Administrator permissions or the {Utility.SORA_ADMIN_ROLE_NAME} role to force vote skip!")
                    .Build()
                );
                return;
            }
            // he has perms so lets toggle it
            using (var soraContext = new SoraContext())
            {
                var guildDb = Utility.GetOrCreateGuild(Context.Guild.Id, soraContext);
                guildDb.NeedVotes = !guildDb.NeedVotes;
                await soraContext.SaveChangesAsync();
                await ReplyAsync("", embed: Utility.ResultFeedback(
                    Utility.GreenSuccessEmbed,
                    Utility.SuccessLevelEmoji[0],
                    $"Successfully turned {(guildDb.NeedVotes ? "ON" : "OFF")} vote skipping.")
                .Build()
                );
            }
        }
        
        [Command("volume"), Alias("vol"), Summary("To set the volume of the player.")]
        public async Task Volume(ushort vol)
        {
            if (!_audio.CheckSameVoiceChannel(Context.Guild.Id, GetVoiceChannelId(Context.User)))
            {
                await ReplyAsync("", embed: Utility.ResultFeedback(
                        Utility.RedFailiureEmbed,
                        Utility.SuccessLevelEmoji[2],
                        "You must be in the same Voice Channel as me!")
                    .Build());
                return;
            }
            await ReplyAsync("", embed: Utility.ResultFeedback(
                    Utility.BlueInfoEmbed,
                    Utility.MusicalNote,
                    await _audio.Volume(Context.Guild.Id, vol))
                .Build());
        }

        [Command("nowplaying"), Alias("now playing", "np"), Summary("Shows the song that is currently playing.")]
        public Task Np()
            => ReplyAsync("", embed: _audio.NowPlaying(Context.Guild.Id));
    }
    
}