using System.Threading.Tasks;
using Discord.Commands;
using SoraBot_v2.Services;

namespace SoraBot_v2.Module
{
    public class GiphyModule : ModuleBase<SocketCommandContext>
    {
        private GiphyService _giphyService;

        public GiphyModule(GiphyService giphyService)
        {
            _giphyService = giphyService;
        }
        
        [Command("gif", RunMode = RunMode.Async), Summary("Gives random Gif with specified search query")]
        public async Task GetRandomGif([Summary("name of gif to search"), Remainder]string query)
        {
            await _giphyService.GetGifBySearch(Context, query);
        }
    }
}