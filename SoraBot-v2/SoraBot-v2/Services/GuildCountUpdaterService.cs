using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace SoraBot_v2.Services
{
    public class GuildCountUpdaterService
    {
        private readonly DiscordSocketClient _client;
        private string token;

        public GuildCountUpdaterService(DiscordSocketClient client)
        {
            _client = client;
            token = ConfigService.GetConfigData("discordbotsToken");
        }
        public async Task UpdateCount()
        {
            using (var httpClient = new HttpClient())
            using (var content = new StringContent($"{{ \"server_count\":{_client.Guilds.Count}}}", Encoding.UTF8, "application/json"))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(token);
                HttpResponseMessage responseMessage =
                    await httpClient.PostAsync("https://discordbots.org/api/bots/270931284489011202/stats", content);
                responseMessage.Dispose(); //TODO CUT IF NOT NEEDED
            }
        }
    }
}