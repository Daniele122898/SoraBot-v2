using System.Threading.Tasks;
using Discord.Commands;
using SoraBot_v2.Services;

namespace SoraBot_v2.Module
{
    [Name("Profile")]
    public class ExpModule : ModuleBase<SocketCommandContext>
    {
        private ExpService _service;

        public ExpModule(ExpService service)
        {
            _service = service;
        }
        
        [Command("togglenotify"), Alias("subscribe", "sub", "subep"), Summary("Will notify you when you level up!")]
        public async Task ToggleNotify()
        {
            await _service.ToggleEpGain(Context);
        }
        
        [Command("top10"), Alias("top", "localtop", "localtop10", "t10", "leaderboard", "localleader", "levels"),
         Summary("Shows the top 10 users in this guild by EXP")]
        public async Task LocalTop10()
        {
            await _service.GetLocalTop10List(Context);
        }
        
        [Command("globaltop10"), Alias("globaltop", "globalt", "top10globally", "gt10", "global top10", "gtop10", "globalleaderboard", "gleaderboard", "alllevels", "gtop100", "gtop", "gleader", "global leaderboard", "globalleaderboard"),
         Summary("Shows the top 10 users globally")]
        public async Task GlobalTop10()
        {
            await _service.GetGlobalTop10(Context);
        }
    }
}