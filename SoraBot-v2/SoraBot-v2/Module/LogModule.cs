using Discord.Commands;
using SoraBot_v2.Services;

namespace SoraBot_v2.Module
{
    public class LogModule : ModuleBase<SocketCommandContext>
    {
        private readonly LogService _logService;

        public LogModule(LogService logService)
        {
            _logService = logService;
        }
    }
}