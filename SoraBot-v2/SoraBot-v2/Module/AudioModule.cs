using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using SoraBot_v2.Services;

namespace SoraBot_v2.Module
{
    public class AudioModule : ModuleBase<SocketCommandContext>
    {
        private AudioService _audio;

        public AudioModule(AudioService service)
        {
            _audio = service;
        }

        private ulong? GetVoiceChannelId(SocketUser user)
            => (user as SocketGuildUser)?.VoiceChannel?.Id;

        [Command("join")]
        public Task Join() 
            => _audio.ConnectAsync(Context.Guild.Id, ((IGuildUser)Context.User), Context.Channel);

        [Command("leave"), Alias("stop")]
        public async Task StopAsync()
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
                    Utility.SuccessLevelEmoji[3],
                    await _audio.DisconnectAsync(Context.Guild.Id))
                .Build());
        }

        [Command("sc", RunMode = RunMode.Async), Alias("soundcloud")]
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

        [Command("yt", RunMode = RunMode.Async), Alias("youtube")]
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

        [Command("play", RunMode = RunMode.Async), Alias("add")]
        public async Task PlayAsync([Remainder] string query)
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
            var info = await _audio.PlayAsync(Context.Guild.Id, query);
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

        [Command("msys"), Alias("musicsys")]
        public Task Msys()
            => ReplyAsync("",
                embed: _audio.PlayerStats(Context.Client.CurrentUser.GetAvatarUrl(), Context.User).Build());

        [Command("pause")]
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
                    _audio.Pause(Context.Guild.Id))
                .Build());
        }

        [Command("resume")]
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
                    _audio.Resume(Context.Guild.Id))
                .Build());
        }

        [Command("queue"), Alias("list")]
        public Task Queue()
            => ReplyAsync("", embed: _audio.DisplayQueue(Context.Guild.Id, Context.User, Context.Channel));

        [Command("clear"), Alias("clearqueue")]
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

        [Command("skip"), Alias("next")]
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

        [Command("volume"), Alias("vol")]
        public async Task Volume(int vol)
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
                    _audio.Volume(Context.Guild.Id, vol))
                .Build());
        }

        [Command("nowplaying"), Alias("now playing", "np")]
        public Task Np()
            => ReplyAsync("", embed: _audio.NowPlaying(Context.Guild.Id));
    }
    
}