using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Weeb.net;

namespace SoraBot.Services.Misc
{
    public class WeebService
    {
        public WeebClient WeebClient { get; private set; }
        public bool IsAuthenticated { get; private set; }

        private readonly ILogger<WeebService> _log;

        public WeebService(ILogger<WeebService> log)
        {
            _log = log;
        }
        
        public async Task<bool> TryAuthenticate(string token)
        {
            WeebClient = new WeebClient("Sora", "3.0.0");
            try
            {
                await WeebClient.Authenticate(token, TokenType.Bearer);
                this.IsAuthenticated = true;
                return true;
            }
            catch (Exception e)
            {
                _log.LogError(e, "Couldn't authenticate WeebClient");
                this.IsAuthenticated = false;
                return false;
            }
        }
    }
}