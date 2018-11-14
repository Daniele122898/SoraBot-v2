using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;
using SoraBot_v2.Services;

namespace SoraBot_v2.Module
{
    [Name("Searches")]
    public class Searches: ModuleBase<SocketCommandContext>
    {
        private GiphyService _giphyService;
        private UbService _ubService;
        private ImdbService _imdbService;
        private WeatherService _weatherService;

        public Searches(GiphyService giphyService, UbService ubService, ImdbService imdbService, WeatherService weatherService)
        {
            _giphyService = giphyService;
            _ubService = ubService;
            _imdbService = imdbService;
            _weatherService = weatherService;
        }
        
        [Command("gif", RunMode = RunMode.Async), Summary("Gives random Gif with specified search query")]
        public async Task GetRandomGif([Summary("name of gif to search"), Remainder]string query)
        {
            await _giphyService.GetGifBySearch(Context, query);
        }
        
        [Command("urbandictionary", RunMode = RunMode.Async), Alias("ub", "ud", "urban")]
        [Summary("Pulls a Urban Dictionary Definition")]
        public async Task GetUbDef([Summary("Definition to search"),Remainder] string urban)
        {
            await _ubService.GetUbDef(Context, urban);
        }
        
        [Command("movie", RunMode = RunMode.Async), Alias("imdb" ,"moviedb"), Summary("Gets Movies/Series from IMDB")]
        public async Task GetImdb([Summary("Movie/Series to search"), Remainder] string target)
        {
            await _imdbService.GetImdb(Context, target);
        }
        
        [Command("chucknorris", RunMode = RunMode.Async), Alias("chuck", "norris"), Summary("Posts a random chuck norris joke")]
        public async Task GetChuckNorris()
        {
            using (var http = new HttpClient())
            {
                string response = await http.GetStringAsync("https://api.chucknorris.io/jokes/random").ConfigureAwait(false);
                var data = JsonConvert.DeserializeObject<NorrisData>(response);
                await Context.Channel.SendMessageAsync("", embed: data.GetEmbed().Build());
            }
        }
        
        [Command("weather", RunMode = RunMode.Async), Summary("Gets the weather of the specified city")]
        public async Task GetWeather([Summary("City to get the weather for"), Remainder]string query)
        {
            await _weatherService.GetWeather(Context, query);
        }
        
        public class NorrisData
        {
            public string icon_url { get; set; }
            public string id { get; set; }
            public string url { get; set; }
            public string value { get; set; }

            public EmbedBuilder GetEmbed() =>
                new EmbedBuilder()
                    .WithColor(Utility.PurpleEmbed)
                    .WithAuthor(x => { x.Name = "Chuck Norris"; x.IconUrl =  ($"{icon_url}"); })
                    .WithUrl( ($"{url}"))
                    .WithDescription($"{value}");
        }
    }
}