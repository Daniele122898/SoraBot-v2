using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SoraBot.Common.Extensions.Hosting
{
    public class BehaviorHost : IHostedService
    {
        private readonly IEnumerable<IBehavior> _behaviors;
        private readonly ILogger<BehaviorHost> _logger;

        public BehaviorHost(IEnumerable<IBehavior> behaviors, ILogger<BehaviorHost> logger)
        {
            _behaviors = behaviors;
            _logger = logger;
        }
        
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting all behaviors");
            await Task.WhenAll(_behaviors
                .Select(async behavior =>
                {
                    await behavior.StartAsync(cancellationToken);
                    _logger.LogInformation("Started behavior {Behavior}", nameof(behavior));
                }));
            _logger.LogInformation("All {Count} behaviors have been started", _behaviors.Count().ToString());
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping all behaviors");
            await Task.WhenAll(_behaviors
                .Select(async behavior =>
                {
                    await behavior.StopAsync(cancellationToken);
                    _logger.LogInformation("Stopped behavior {Behavior}", nameof(behavior));
                }));
            _logger.LogInformation("All {Count} behaviors have been stopped", _behaviors.Count().ToString());
        }
    }
}