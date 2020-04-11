using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SoraBot.Bot
{
    public sealed class SoraBot : BackgroundService
    {
        private readonly ILogger<SoraBot> _logger;

        public SoraBot(ILogger<SoraBot> logger)
        {
            _logger = logger;
        }
        
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("SoraBot Background task is starting...");

            stoppingToken.Register(OnTokenStop);

            return Task.CompletedTask;
        }

        private void OnTokenStop()
        {
            _logger.LogInformation("Stopping background service.");
        }
    }
}