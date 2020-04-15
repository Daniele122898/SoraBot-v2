using System.Threading;
using System.Threading.Tasks;

namespace SoraBot.Common.Extensions.Hosting
{
    /// <summary>
    /// Individual behaviors that are managed by the BehaviorHost
    /// </summary>
    public interface IBehavior
    {
        Task StartAsync(CancellationToken cancellationToken);

        Task StopAsync(CancellationToken cancellationToken);
    }
}