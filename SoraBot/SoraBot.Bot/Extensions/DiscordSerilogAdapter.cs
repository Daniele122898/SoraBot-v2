using System.Threading.Tasks;
using Discord;
using Microsoft.Extensions.Logging;

namespace SoraBot.Bot.Extensions
{
    // This is more or less copied from the Modix bot. It's just a great way to combine serilog and sentry for discord :)
    public class DiscordSerilogAdapter
    {
        private readonly ILogger<DiscordSerilogAdapter> _log;

        public DiscordSerilogAdapter(ILogger<DiscordSerilogAdapter> log)
        {
            _log = log;
        }
        
        public Task HandleLog(LogMessage message)
        {
            switch (message.Severity)
            {
                case LogSeverity.Critical:
                    _log.LogCritical(message.Exception, message.ToString() ?? "An exception bubbled up: ");
                    break;
                case LogSeverity.Debug:
                    _log.LogDebug(message.ToString());
                    break;
                case LogSeverity.Warning:
                    if (message.Exception == null)
                        _log.LogWarning(message.ToString());
                    else
                        _log.LogWarning(message.Exception, message.ToString());
                    break;
                case LogSeverity.Error:
                    _log.LogError(message.Exception, message.ToString() ?? "An exception bubbled up: ");
                    break;
                case LogSeverity.Info:
                    _log.LogInformation(message.ToString());
                    break;
                case LogSeverity.Verbose:
                    _log.LogTrace(message.ToString());
                    break;
            }

            return Task.CompletedTask;
        }
    }
}