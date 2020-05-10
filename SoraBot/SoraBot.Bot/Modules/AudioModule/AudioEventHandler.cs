using Microsoft.Extensions.Logging;

namespace SoraBot.Bot.Modules.AudioModule
{
    public class AudioEventHandler
    {
        private readonly ILogger<AudioEventHandler> _log;

        public AudioEventHandler(ILogger<AudioEventHandler> log)
        {
            log.LogInformation("Initialized Audio Event Handlers");
            _log = log;
        }
    }
}