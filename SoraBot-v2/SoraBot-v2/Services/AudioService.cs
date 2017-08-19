using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.WebSocket;
using SoraBot_v2.Data;
using SoraBot_v2.Data.Entities.SubEntities;
using SoraBot_v2.Extensions;

namespace SoraBot_v2.Services
{
    public class AudioService
    {
        private readonly ConcurrentDictionary<ulong, AudioData> _connectedGuilds = new ConcurrentDictionary<ulong, AudioData>();
        private readonly ConcurrentDictionary<ulong, Queue<Song>> _guildQueue = new ConcurrentDictionary<ulong, Queue<Song>>();
        private SoraContext _soraContext;

        public AudioService(SoraContext soraContext)
        {
            _soraContext = soraContext;
        }

        public async Task JoinVc(SocketGuild guild, IVoiceChannel voiceChannel)
        {
            AudioData audioData;
            if (_connectedGuilds.TryGetValue(guild.Id, out audioData))
            {
                return;
            }
            if(voiceChannel.Guild.Id != guild.Id)
                return;
            audioData = new AudioData
            {
                AudioClient = await voiceChannel.ConnectAsync(),
                CancellationTokenSource = new CancellationTokenSource()
            };
            
            if (_connectedGuilds.TryAdd(guild.Id, audioData))
            {
                Console.WriteLine($"[{DateTime.Now.TimeOfDay}] Connected to VC: {voiceChannel.Name} on {guild.Name}");
            }
        }

        public async Task LeaveVc(SocketCommandContext context)
        {
            AudioData audioData = new AudioData();
            if (!_connectedGuilds.TryRemove(context.Guild.Id, out audioData))
            {
                await context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                        "Sora isn't connected to any VC in this guild"));
                return;
            }
            await audioData.AudioClient.StopAsync();
            audioData.AudioClient.Dispose();//TODO CANCEL?
            Console.WriteLine($"[{DateTime.Now.TimeOfDay}] Disconnected from VC on {context.Guild.Name}");
        }

        public async Task ToggleRepeat(SocketCommandContext context)
        {
            AudioData audioData;
            if (!_connectedGuilds.TryGetValue(context.Guild.Id, out audioData))
            {
                await context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                        "Sora is not connected to any VC in this guild!"));
                return;
            }
            audioData.Repeat = !audioData.Repeat;
            _connectedGuilds.TryUpdate(context.Guild.Id, audioData);
            if (audioData.Repeat)
                await context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0],
                        "Set repeat to ON"));
            else
                await context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0],
                        "Set repeat to OFF"));
        }

        public async Task AddMusicToQueue(SocketCommandContext context, string path)
        {
            Queue<Song> currentQueue;
            if (!_guildQueue.TryGetValue(context.Guild.Id, out currentQueue))
                currentQueue = new Queue<Song>();
            string encodedPath = StringEncoder.Base64Encode(path);
            Song addSong = new Song()
            {
                Added = DateTime.UtcNow,
                Base64EncodedLink = encodedPath,
                Name = "Test",//TODO NAME
                RequestorUserId = context.User.Id
            };
            currentQueue.Enqueue(addSong);
            _guildQueue.AddOrUpdate(context.Guild.Id, currentQueue, (key, oldValue) => currentQueue);

            await context.Channel.SendMessageAsync("",
                embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0],
                    "Successfully added song to queue"));

        }

        public async Task PlayMusicAsync(SocketCommandContext context)
        {
            AudioData audioData = new AudioData();
            if (_connectedGuilds.TryGetValue(context.Guild.Id, out audioData))
            {
                Queue<Song> currentQueue;
                if (!_guildQueue.TryGetValue(context.Guild.Id, out currentQueue))
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            "You have not added any songs to the queue yet!"));
                    return;
                }
                if (currentQueue.Count < 1)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            "The queue is empty"));
                    return;
                }
                Song nextSong = currentQueue.Peek();//for repeat function to work!
                string path = nextSong.Base64EncodedLink;
                
                if (!File.Exists(path+".mp3"))
                {
                    await context.Channel.SendMessageAsync("", embed:Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"The item in the queue does not exist anymore! Songs get deleted after a month. Please skip current song!"));
                    return;
                }
                
                await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed,Utility.SuccessLevelEmoji[0], $"Started playing {nextSong.Name}"));
                if(audioData.CancellationTokenSource == null)
                    audioData.CancellationTokenSource = new CancellationTokenSource();
                var output = CreateStream(path).StandardOutput.BaseStream;
                Console.WriteLine($"[{DateTime.Now.TimeOfDay}] Playing {nextSong.Name} in {context.Guild.Name}");
                var stream = audioData.AudioClient.CreatePCMStream(AudioApplication.Music,bufferMillis:500);
                await output.CopyToAsync(stream, 500, audioData.CancellationTokenSource.Token);
                await stream.FlushAsync().ConfigureAwait(false);
                audioData.CancellationTokenSource.Cancel();
                audioData.CancellationTokenSource.Dispose();
                await output.FlushAsync();
                output.Dispose();
                stream.Dispose();
                if (!audioData.Repeat)
                    currentQueue.Dequeue();//remove song if repeat was not on!

                if (currentQueue.Count > 0)
                {
                    audioData.CancellationTokenSource = new CancellationTokenSource();
                    _connectedGuilds.TryUpdate(context.Guild.Id, audioData);
                    PlayMusicAsync(context);
                }
                else
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.PurpleEmbed, Utility.SuccessLevelEmoji[4], "🎵 Stopped playback since queue is empty"));
                }
            }
            else
            {
                await context.Channel.SendMessageAsync("", embed:Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Sora is not connected to any VC on this guild!"));
                return;
            }
        }

        public async Task StopMusic(SocketCommandContext context)
        {
            AudioData audioData = new AudioData();
            if (!_connectedGuilds.TryGetValue(context.Guild.Id, out audioData))
            {
                await context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                        "Sora is not connected to any VC in this guild!"));
                return;
            }
            audioData.CancellationTokenSource.Cancel();
            audioData.CancellationTokenSource.Dispose();
            audioData.CancellationTokenSource = new CancellationTokenSource();
            _connectedGuilds.TryUpdate(context.Guild.Id, audioData);
            await context.Channel.SendMessageAsync("",
                embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0], "Stopped Playback"));
        }

        private Process CreateStream(string path)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-i \"{path}.mp3\" -ac 2 -loglevel panic -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true
            });
        }
    }

}