using System.Threading.Tasks;
using Discord.Commands;
using Weeb.net;

namespace SoraBot_v2.Services
{
    public class WeebService
    {
        private WeebClient _weebClient;
        private string _token;

        public WeebService()
        {
            _token = ConfigService.GetConfigData("weebToken");
            _weebClient = new WeebClient();
        }
        
        public async Task InitializeAsync()
        {
            _weebClient.Authenticate(_token);
        }

        public async Task GetTypes(SocketCommandContext context)
        {
            var result = await _weebClient.GetTypesAsync();
            string types = "";
            foreach (var resultType in result.Types)
            {
                types += $"{resultType}, ";
            }
            await context.Channel.SendMessageAsync($"```\n{types}\n```");
        }

    }
}