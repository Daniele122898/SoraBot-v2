using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using SoraBot_v2.Services;

namespace SoraBot_v2.Module
{
    public class AudioModule2 : ModuleBase<SocketCommandContext>
    {
        public AudioService2 Audio { get; set; }

        public AudioModule2(AudioService2 service2)
        {
            Audio = service2;
        }

        [Command("Join")]
        public Task Join()
            => Audio.ConnectAsync(Context.Guild.Id, (IGuildUser)Context.User, Context.Channel);

        [Command("Leave"), Alias("Stop")]
        public async Task StopAsync()
            => await ReplyAsync(await Audio.StopAsync(Context.Guild.Id));

        [Command("Leave")]
        public async Task Leave()
            => await ReplyAsync(await Audio.DisconnectAsync(Context.Guild.Id));

        [Command("Play")]
        public async Task PlayAsync([Remainder] string query)
            => await ReplyAsync(await Audio.PlayAsync(Context.Guild.Id, query));

        [Command("Pause")]
        public Task Pause()
            => ReplyAsync(Audio.Pause(Context.Guild.Id));

        [Command("Resume")]
        public Task Resume()
            => ReplyAsync(Audio.Resume(Context.Guild.Id));

        [Command("Queue")]
        public Task Queue()
            => ReplyAsync(Audio.DisplayQueue(Context.Guild.Id));

        [Command("Seek")]
        public Task Seek(TimeSpan span)
            => ReplyAsync(Audio.Seek(Context.Guild.Id, span));

        [Command("Skip")]
        public async Task SkipAsync()
            => await ReplyAsync(await Audio.SkipAsync(Context.Guild.Id, Context.User.Id));

        [Command("Volume")]
        public Task Volume(int volume)
            => ReplyAsync(Audio.Volume(Context.Guild.Id, volume));
    
    }
}