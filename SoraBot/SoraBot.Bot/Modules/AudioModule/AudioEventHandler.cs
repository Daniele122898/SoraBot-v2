using System;
using System.Threading.Tasks;
using Discord;
using Microsoft.Extensions.Logging;
using SoraBot.Common.Extensions.Modules;
using SoraBot.Common.Utils;
using Victoria;
using Victoria.EventArgs;

namespace SoraBot.Bot.Modules.AudioModule
{
    public class AudioEventHandler : IDisposable
    {
        private readonly ILogger<AudioEventHandler> _log;
        private readonly LavaNode _node;
        private readonly AudioStatsService _audioStatsService;

        public AudioEventHandler(
            ILogger<AudioEventHandler> log, 
            LavaNode node,
            AudioStatsService audioStatsService)
        {
            log.LogInformation("Initialized Audio Event Handlers");
            _log = log;
            _node = node;
            _audioStatsService = audioStatsService;

            _node.OnLog += OnLog;
            _node.OnTrackEnded += OnTrackEnded;
            _node.OnTrackStuck += OnTrackStuck;
            _node.OnTrackException += OnTrackException;
            _node.OnWebSocketClosed += OnWebSocketClosed;
            _node.OnStatsReceived += OnStatsReceived;
        }

        private Task OnStatsReceived(StatsEventArgs e)
        {
            _audioStatsService.SetStats(e);
            return Task.CompletedTask;
        }

        private Task OnWebSocketClosed(WebSocketClosedEventArgs e)
        {
            _log.LogWarning("Websocket connection lost from LavaNode");
            return Task.CompletedTask;
        }

        private async Task OnTrackException(TrackExceptionEventArgs e)
        {
            if (e.Player == null || e.Track == null) 
                return;   
            
            e.Player.Queue.Remove(e.Track);

            var eb = this.GetSimpleMusicEmbed("Track threw and exception. Attempting to play next track");

            string desc = $"**{e.Track.Title}**";
            if (!string.IsNullOrWhiteSpace(e.ErrorMessage))
                desc += $"\n{e.ErrorMessage}";
            eb.WithDescription(desc);
            
            await e.Player.TextChannel.SendMessageAsync(
                embed: eb.Build());
        }

        private async Task OnTrackStuck(TrackStuckEventArgs e)
        {
            if (e.Player == null || e.Track == null) 
                return;   
            
            e.Player.Queue.Remove(e.Track);

            await e.Player.TextChannel.SendMessageAsync(
                embed: this.GetSimpleMusicEmbed("Track got stuck. Attempting to play next track")
                    .WithDescription(e.Track.Title).Build());
        }

        private EmbedBuilder GetSimpleMusicEmbed(string message)
            => new EmbedBuilder()
            {
                Color = SoraSocketCommandModule.Blue,
                Title = $"{SoraSocketCommandModule.MusicalNote} {message}"
            };

        private async Task<EmbedBuilder> GetExtendedMusicEmbed(LavaTrack track)
        {
            var eb = new EmbedBuilder()
            {
                Color = SoraSocketCommandModule.Blue,
                Title = $"{SoraSocketCommandModule.MusicalNote} Next: [{Formatter.FormatTime(track.Duration)}] - **{track.Title}**",
                Footer = new EmbedFooterBuilder()
                {
                    Text = $"Video by {track.Author}"
                },
                Url = track.Url,
            };
            var imageUrl = await track.FetchArtworkAsync();
            if (!string.IsNullOrWhiteSpace(imageUrl))
                eb.WithThumbnailUrl(imageUrl);
            return eb;
        }

        private async Task OnTrackEnded(TrackEndedEventArgs e)
        {
            if (!e.Reason.ShouldPlayNext() || e.Player == null)
                return;

            if (!e.Player.Queue.TryDequeue(out var queueable))
            {
                await e.Player.TextChannel.SendMessageAsync(embed: this.GetSimpleMusicEmbed("No more tracks in queue.").Build());
                return;
            }

            if (!(queueable is LavaTrack track))
            {
                await e.Player.TextChannel.SendMessageAsync(
                    embed: this.GetSimpleMusicEmbed("Next item in queue was not a track. Stopped playback..").Build());
                return;
            }
            
            // Queue next song
            await e.Player.PlayAsync(track);
            var eb = await this.GetExtendedMusicEmbed(track);
            await e.Player.TextChannel.SendMessageAsync(embed: eb.Build());
        }

        private Task OnLog(LogMessage log)
        {
            switch (log.Severity)
            {
                case LogSeverity.Critical:
                    _log.LogWarning(log.Exception, log.Message);
                    break;
                case LogSeverity.Error:
                    _log.LogWarning(log.Exception, log.Message);
                    break;
                case LogSeverity.Warning:
                    _log.LogWarning(log.Exception, log.Message);
                    break;
                case LogSeverity.Info:
                    _log.LogInformation(log.Message);
                    break;
                case LogSeverity.Verbose:
                    _log.LogTrace(log.Message);
                    break;
                case LogSeverity.Debug:
                    _log.LogDebug(log.Message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _node.OnLog -= OnLog;
            _node.OnTrackEnded -= OnTrackEnded;
            _node.OnTrackStuck -= OnTrackStuck;
            _node.OnTrackException -= OnTrackException;
            _node.OnWebSocketClosed -= OnWebSocketClosed;
            _node.OnStatsReceived -= OnStatsReceived;
        }
    }
}