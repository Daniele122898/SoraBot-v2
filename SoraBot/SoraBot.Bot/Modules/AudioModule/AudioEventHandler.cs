using System;
using System.Threading.Tasks;
using Discord;
using Microsoft.Extensions.Logging;
using Victoria;
using Victoria.EventArgs;

namespace SoraBot.Bot.Modules.AudioModule
{
    public class AudioEventHandler : IDisposable
    {
        private readonly ILogger<AudioEventHandler> _log;
        private readonly LavaNode _node;

        public AudioEventHandler(ILogger<AudioEventHandler> log, LavaNode node)
        {
            log.LogInformation("Initialized Audio Event Handlers");
            _log = log;
            _node = node;
            
            _node.OnLog += OnLog;
            _node.OnTrackEnded += OnTrackEnded;
            _node.OnTrackStuck += OnTrackStuck;
            _node.OnTrackException += OnTrackException;
            _node.OnWebSocketClosed += OnWebSocketClosed;
            _node.OnStatsReceived += OnStatsReceived;
        }

        private Task OnStatsReceived(StatsEventArgs arg)
        {
            throw new NotImplementedException();
        }

        private Task OnWebSocketClosed(WebSocketClosedEventArgs arg)
        {
            throw new NotImplementedException();
        }

        private Task OnTrackException(TrackExceptionEventArgs arg)
        {
            throw new NotImplementedException();
        }

        private Task OnTrackStuck(TrackStuckEventArgs arg)
        {
            throw new NotImplementedException();
        }

        private Task OnTrackEnded(TrackEndedEventArgs arg)
        {
            throw new NotImplementedException();
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