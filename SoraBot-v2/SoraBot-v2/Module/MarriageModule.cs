using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using SoraBot_v2.Services;

namespace SoraBot_v2.Module
{
    public class MarriageModule : ModuleBase<SocketCommandContext>
    {
        private MarriageService _marriageService;

        public MarriageModule(MarriageService marriageService)
        {
            _marriageService = marriageService;
        }

        [Command("marry", RunMode = RunMode.Async), Summary("Marries specified person if agreed")]
        public async Task Marry(SocketUser user)
        {
            await _marriageService.Marry(Context, user);
        }

        [Command("marrylimit"), Alias("checklimit"), Summary("Checks your marriage limit")]
        public async Task MarriageLimit(SocketUser userT = null)
        {
            var user = userT ?? Context.User;
            await _marriageService.CheckLimit(Context, user);
        }
    }
}