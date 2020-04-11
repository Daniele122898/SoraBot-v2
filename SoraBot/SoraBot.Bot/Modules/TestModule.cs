using System.Threading.Tasks;
using Discord.Commands;
using SoraBot.Common.Extensions.Modules;

namespace SoraBot.Bot.Modules
{
    public class TestModule : SoraSocketCommandModule
    {
        [Command("test")]
        public Task TestMessage() => ReplyDefaultEmbed("I work :>");
    }
}