using System.Threading.Tasks;
using Discord.WebSocket;

namespace SoraBot.Services.Profile
{
    public interface IExpService
    {
        Task TryGiveUserExp(SocketMessage msg, SocketGuildChannel channel);
    }
}