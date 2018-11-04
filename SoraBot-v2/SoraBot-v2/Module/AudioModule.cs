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

        [Command("join", RunMode = RunMode.Async)]
        public Task Join() 
            => _audio.ConnectAsync(Context.Guild.Id, ((IGuildUser)Context.User), Context.Channel);

        [Command("leave"), Alias("stop")]
        public async Task StopAsync()
            => await ReplyAsync("", embed: Utility.ResultFeedback(
                    Utility.BlueInfoEmbed,
                    Utility.SuccessLevelEmoji[3],
                    await _audio.DisconnectAsync(Context.Guild.Id))
                .Build());

        [Command("play", RunMode = RunMode.Async), Alias("add")]
        public async Task PlayAsync([Remainder] string query)
        {
            var info = await _audio.PlayAsync(Context.Guild.Id, query);
            await ReplyAsync("", embed: Utility.ResultFeedback(
                    Utility.GreenSuccessEmbed,
                    Utility.MusicalNote,
                    $"{(info.enqued ? "Enqueued" : "Playing")}: [{info.track.Length.ToString(@"mm\:ss")}] - **{info.track.Title}**")
                .WithUrl(info.track.Uri.ToString())
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

        [Command("skip", RunMode = RunMode.Async), Alias("next")]
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