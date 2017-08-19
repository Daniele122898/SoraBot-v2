using System.Threading.Tasks;
using Discord.Commands;
using SoraBot_v2.Services;

namespace SoraBot_v2.Module
{
    public class Searches: ModuleBase<SocketCommandContext>
    {
        private GiphyService _giphyService;
        private UbService _ubService;
        private ImdbService _imdbService;

        public Searches(GiphyService giphyService, UbService ubService, ImdbService imdbService)
        {
            _giphyService = giphyService;
            _ubService = ubService;
            _imdbService = imdbService;
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
    }
}