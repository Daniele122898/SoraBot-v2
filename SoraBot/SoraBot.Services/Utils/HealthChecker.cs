using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SoraBot.Services.Utils
{
    public class HealthChecker
    {
        private readonly IHostApplicationLifetime _applicationLifetime;
        private readonly DiscordSocketClient _client;
        private readonly ILogger<HealthChecker> _log;
        private readonly Timer _timer;

        private int _disconnectedCounter = 0;

        public HealthChecker(
            IHostApplicationLifetime applicationLifetime,
            DiscordSocketClient client,
            ILogger<HealthChecker> log)
        {
            _applicationLifetime = applicationLifetime;
            _client = client;
            _log = log;
            _timer = new Timer(HealthCheck, null, TimeSpan.FromMinutes(30),
                TimeSpan.FromMinutes(30));
            
            _client.Disconnected += ClientOnDisconnected;
            _log.LogInformation("Initialized HealthChecker");
        }

        private Task ClientOnDisconnected(Exception ex)
        {
            _log.LogWarning(ex, "Client disconnected unexpectedly, increasing health monitoring...");
            _timer.Change(TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(2));
            return Task.CompletedTask;
        }

        private void HealthCheck(object state)
        {
            if (_client.ConnectionState == ConnectionState.Connected)
            {
                _log.LogTrace("Connection state came out okey. Resetting timer to 30 minutes check");
                // This means the last check was also successful so let's decrease the amount of operations and memory r/w :)
                if (_disconnectedCounter == 0) return;
                
                _disconnectedCounter = 0;
                _timer.Change(TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(30));
                return;
            }
            // We're not connected
            _disconnectedCounter++;
            if (_disconnectedCounter < 3) return;
            
            // Shut it down and let the systemd service restart it bcs d.net prolly threadlocked itself :)
            _log.LogError("Socket client failed to reset itself. Shutting down application...");
            _client.Disconnected -= ClientOnDisconnected;
            GlobalConstants.ApplicationCancellationTokenSource.Cancel();
            _applicationLifetime.StopApplication();

        }
    }
}