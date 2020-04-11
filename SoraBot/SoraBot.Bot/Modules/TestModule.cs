using System.Threading.Tasks;
using Discord.Commands;

namespace SoraBot.Bot.Modules
{
    public class TestModule : ModuleBase<SocketCommandContext>
    {
        [Command("test")]
        public Task TestMessage() => ReplyAsync("I work :>");
    }
}