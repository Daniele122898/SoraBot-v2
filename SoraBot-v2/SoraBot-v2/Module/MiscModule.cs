using System.Net.Sockets;
using System.Threading.Tasks;
using Discord.Commands;

namespace SoraBot_v2.Module
{
    public class MiscModule : ModuleBase<SocketCommandContext>
    {
        [Command("ping"), Summary("Gives the latency of the Bot to the Discord API")]
        public async Task Ping()
        {
            await ReplyAsync($"Pong! {Context.Client.Latency} ms :ping_pong:");
        }

        [Command("exc")]
        [RequireOwner]
        public async Task ThrowException()
        {
            int i2 = 0;
            int i = 10 / i2;
        }
    }
}