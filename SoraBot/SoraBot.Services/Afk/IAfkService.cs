using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace SoraBot.Services.Afk
{
    public interface IAfkService
    {
        public Task CheckUserAfkStatus(SocketGuildChannel channel, IUser user);
    }
}