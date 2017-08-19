using System.Threading.Tasks;
using Discord.Commands;
using SoraBot_v2.Services;

namespace SoraBot_v2.Module
{
    public class Searches: ModuleBase<SocketCommandContext>
    {
        private GiphyService _giphyService;
        private UbService _ubService;

        public Searches(GiphyService giphyService, UbService ubService)
        {
            _giphyService = giphyService;
            _ubService = ubService;
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
    }
}