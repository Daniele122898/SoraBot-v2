using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace SoraBot_v2.Services
{
    public class GuildCountUpdaterService
    {
        private readonly string token;

        public GuildCountUpdaterService()
        {
            token = ConfigService.GetConfigData("discordbotsToken");
        }
        
        public async Task UpdateCount(int guildCount)
        {
            using (var httpClient = new HttpClient())
            using (var request = new HttpRequestMessage(HttpMethod.Post, "https://discordbots.org/api/bots/270931284489011202/stats"))
            {
                string json = JsonConvert.SerializeObject(new { server_count = guildCount});
                request.Content = new StringContent(json);
                request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                request.Headers.Add("Authorization", token);
                HttpResponseMessage response = await httpClient.SendAsync(request);
                response.Dispose(); //TODO CUT IF NOT NEEDED
            }
        }
    }
}