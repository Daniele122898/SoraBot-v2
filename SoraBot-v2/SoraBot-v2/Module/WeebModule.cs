using System.Threading.Tasks;
using Discord.Commands;
using SoraBot_v2.Services;

namespace SoraBot_v2.Module
{
    [RequireOwner]
    public class WeebModule : ModuleBase<SocketCommandContext>
    {
        private WeebService _weebService;

        public WeebModule(WeebService service)
        {
            _weebService = service;
        }

        [Command("weebtypes")]
        public async Task WeebTypes()
        {
            await _weebService.GetTypes(Context);
        }

        [Command("weebtags")]
        public async Task WeebTags()
        {
            await _weebService.GetTags(Context);
        }

        [Command("weebrantype")]
        public async Task WeebRandomType(string type)
        {
            await _weebService.GetImages(Context, type, new string[]{});
        }
        
        [Command("weebrantag")]
        public async Task WeebRandomTag(string tag)
        {
            string[] tags = tag.Split(',');
            await _weebService.GetImages(Context, "", tags);
        }
    }
}