using System.Threading.Tasks;
using Discord.Commands;
using SoraBot_v2.Services;

namespace SoraBot_v2.Module
{
    [Name("Anime / Manga")]
    public class AnimeSearchModule :ModuleBase<SocketCommandContext>
    {
        private AnimeSearchService _animeSearchService;

        public AnimeSearchModule(AnimeSearchService service)
        {
            _animeSearchService = service;
        }
        
        [Command("anime", RunMode = RunMode.Async), Summary("Gets the stats of your desired Anime")]
        public async Task GetAnime([Summary("Anime to search"), Remainder]string anime)
        {
            await _animeSearchService.GetInfo(Context, anime.Replace(":", " "), AnimeSearchService.AnimeType.Anime);
        }

        [Command("manga", RunMode = RunMode.Async), Summary("Gets the stats of your desired Manga")]
        public async Task GetManga([Summary("Manga to search"), Remainder]string manga)
        {
            await _animeSearchService.GetInfo(Context, manga.Replace(":", " "), AnimeSearchService.AnimeType.Manga);
        }

        [Command("character", RunMode = RunMode.Async), Alias("char"), Summary("Gets the stats of your desired Character")]
        public async Task GetChar([Summary("Character to search"), Remainder]string charName)
        {
            await _animeSearchService.GetInfo(Context, charName.Replace(":", " "), AnimeSearchService.AnimeType.Char);
        }
    }
}