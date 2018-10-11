using System;
using System.Net.Http;
using System.Threading.Tasks;
using Discord.Commands;
using Newtonsoft.Json;
using SoraBot_v2.Data.Entities.SubEntities;

namespace SoraBot_v2.Services
{
    public class WeatherService
    {
        private string _weatherId;

        public WeatherService()
        {
            _weatherId = ConfigService.GetConfigData("weather");
        }

        public async Task GetWeather(SocketCommandContext context, string query)
        {
            try
            {
                var search = System.Net.WebUtility.UrlEncode(query);
                string response = "";
                using (var http = new HttpClient())
                {
                    response = await http.GetStringAsync($"http://api.openweathermap.org/data/2.5/weather?q={Uri.EscapeUriString(search)}&appid={_weatherId}&units=metric").ConfigureAwait(false);
                }
                var data = JsonConvert.DeserializeObject<WeatherData>(response);

                await context.Channel.SendMessageAsync("", embed: data.GetEmbed().Build());
            }
            catch (Exception)
            {
                await context.Channel.SendMessageAsync("", embed:
                    Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Couldn't find weather for specified location!").Build());
            }
        }
    }
}