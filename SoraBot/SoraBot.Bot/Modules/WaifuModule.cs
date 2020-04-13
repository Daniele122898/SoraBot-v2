using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using SoraBot.Common.Extensions.Modules;
using SoraBot.Services.Waifu;

namespace SoraBot.Bot.Modules
{
    public class WaifuModule : SoraSocketCommandModule
    {
        private readonly IWaifuService _waifuService;

        public WaifuModule(IWaifuService waifuService)
        {
            _waifuService = waifuService;
        }

        [Command("waifu")]
        public async Task OpenWaifuBox()
        {
            var eb = new EmbedBuilder()
            {
                Title = "TEST BOX"
            };

            for (int i = 0; i < 3; i++)
            {
                var waifu = await _waifuService.GetRandomWaifu();
                eb.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = waifu.Name;
                    x.Value = $"_ID: {waifu.Id}_";
                });
            }

            await ReplyAsync("", embed: eb.Build());
        }
    }
}