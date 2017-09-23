using System.Threading.Tasks;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;

namespace SoraBot_v2.Extensions
{
    public class EnsureFromUserInChannel : ICriterion<SocketMessage>
    {
        private readonly ulong _userId;
        private readonly ulong _channelId;

        public EnsureFromUserInChannel(ulong userId, ulong channelId)
        {
            _userId = userId;
            _channelId = channelId;
        }
        
        public Task<bool> JudgeAsync(SocketCommandContext sourceContext, SocketMessage parameter)
        {
            return Task.FromResult<bool>(parameter.Author.Id == _userId && parameter.Channel.Id == _channelId);
        }
    }
}