using System.Threading.Tasks;
using Discord.Commands;
using SoraBot_v2.Services;

namespace SoraBot_v2.Module
{
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
    }
}