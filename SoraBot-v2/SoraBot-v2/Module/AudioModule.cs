using System.Threading.Tasks;
using Discord;
using Discord.Commands;
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

        [Command("join")]
        public Task Join() 
            => _audio.ConnectAsync(Context.Guild.Id, ((IGuildUser)Context.User), Context.Channel);

        [Command("leave"), Alias("stop")]
        public async Task StopAsync()
            => await ReplyAsync("", embed: Utility.ResultFeedback(
                    Utility.BlueInfoEmbed,
                    Utility.SuccessLevelEmoji[3],
                    await _audio.DisconnectAsync(Context.Guild.Id))
                .Build());
        
        [Command("sc", RunMode = RunMode.Async), Alias("soundcloud")]
        public async Task ScSearch([Remainder] string query)
            => await _audio.YoutubeOrSoundCloudSearch(Context, query, false);      

        [Command("yt", RunMode = RunMode.Async), Alias("youtube")]
        public async Task YtSearch([Remainder] string query)
            => await _audio.YoutubeOrSoundCloudSearch(Context, query, true);            

        [Command("play", RunMode = RunMode.Async), Alias("add")]
        public async Task PlayAsync([Remainder] string query)
        {
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

        [Command("pause")]
        public Task Pause()
            => ReplyAsync("", embed: Utility.ResultFeedback(
                    Utility.BlueInfoEmbed,
                    Utility.MusicalNote,
                    _audio.Pause(Context.Guild.Id))
                .Build());
        
        [Command("resume")]
        public Task Resume()
            => ReplyAsync("", embed: Utility.ResultFeedback(
                    Utility.BlueInfoEmbed,
                    Utility.MusicalNote,
                    _audio.Resume(Context.Guild.Id))
                .Build());

        [Command("queue"), Alias("list")]
        public Task Queue()
            => ReplyAsync("", embed: _audio.DisplayQueue(Context.Guild.Id, Context.User, Context.Channel));

        [Command("skip"), Alias("next")]
        public async Task SkipAsync()
            => await ReplyAsync("", embed: await _audio.SkipAsync(Context.Guild.Id, Context.User.Id));
        
        [Command("volume"), Alias("vol")]
        public Task Volume(int vol)
            => ReplyAsync("", embed: Utility.ResultFeedback(
                    Utility.BlueInfoEmbed,
                    Utility.MusicalNote,
                    _audio.Volume(Context.Guild.Id, vol))
                .Build());

        [Command("nowplaying"), Alias("now playing", "np")]
        public Task Np()
            => ReplyAsync("", embed: _audio.NowPlaying(Context.Guild.Id));
    }
    
}