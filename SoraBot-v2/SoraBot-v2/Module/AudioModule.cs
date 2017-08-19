using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using SoraBot_v2.Services;

namespace SoraBot_v2.Module
{
    public class AudioModule : ModuleBase<SocketCommandContext>
    {
        private readonly AudioService _audioService;
        /*
        public AudioModule(AudioService service)
        {
            _audioService = service;
        }*/

        [Command("join", RunMode = RunMode.Async), Summary("Joins your Voice channel")]
        public async Task JoinVc()
        {
            await DisabledNotice(Context);
            return;
            var voiceState = Context.User as IVoiceState;
            if (voiceState != null)
            {
                if (voiceState.VoiceChannel == null)
                {
                    await ReplyAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "You are not connected to a Voice Channel!"));
                    return;
                }
                await _audioService.JoinVc(Context.Guild, voiceState.VoiceChannel);
            }
        }

        [Command("leave", RunMode = RunMode.Async), Summary("Leaves your Voice channel")]
        public async Task LeaveVc()
        {
            await DisabledNotice(Context);
            return;
            await _audioService.LeaveVc(Context);
        }

        [Command("play", RunMode = RunMode.Async)]
        public async Task PlayMusic()
        {
            await DisabledNotice(Context);
            return;
            await _audioService.PlayMusicAsync(Context);
        }

        [Command("repeat")]
        public async Task RepeatSong()
        {
            await DisabledNotice(Context);
            return;
            await _audioService.ToggleRepeat(Context);
        }

        [Command("add", RunMode = RunMode.Async)]
        public async Task AddSong([Remainder] string song)
        {
            await DisabledNotice(Context);
            return;
            await _audioService.AddMusicToQueue(Context, song);
        }

        [Command("stop", RunMode = RunMode.Async)]
        public async Task StopMusic()
        {
            await DisabledNotice(Context);
            return;
            await _audioService.StopMusic(Context);
        }

        private async Task DisabledNotice(SocketCommandContext context)
        {
            await context.Channel.SendMessageAsync("",
                embed: Utility.ResultFeedback(Utility.YellowWarningEmbed, Utility.SuccessLevelEmoji[1],
                    "All music features are currently disabled due to a memory leak within the discord.net library!"));
        }
    }
}