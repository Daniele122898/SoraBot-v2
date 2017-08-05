using System.Threading.Tasks;
using Discord.Commands;
using SoraBot_v2.Data;
using SoraBot_v2.Services;

namespace SoraBot_v2.Module
{
    public class AfkModule : ModuleBase<SocketCommandContext>
    {
        private AfkService _afkService;
        private SoraContext _soraContext;

        public AfkModule(AfkService afkService, SoraContext soraContext)
        {
            _afkService = afkService;
            _soraContext = soraContext;
        }
        
        [Command("afk"), Alias("away"), Summary("Sets you AFK with a specified message to deliver to anyone that mentions you")]
        public async Task ToggleAFK([Summary("Message to deliver when you get mentioned"), Remainder]string msg = "")
        {
            await _afkService.ToggleAFK(Context, msg, _soraContext);
        }
    }
}