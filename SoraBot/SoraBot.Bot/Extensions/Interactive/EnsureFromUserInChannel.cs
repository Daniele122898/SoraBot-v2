using System.Threading.Tasks;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;

namespace SoraBot.Bot.Extensions.Interactive
{
    /// <summary>
    /// This Criterion is to ensure that if a message is sent via the <see cref="InteractiveService"/>
    /// it will only trigger if a response is created by the specified userId and in the specified channel.
    /// This should be used if input from a specific user is needed
    /// </summary>
    public class EnsureFromUserInChannel : ICriterion<SocketMessage>
    {
        private readonly ulong _userId;
        private readonly ulong _channelId;

        public EnsureFromUserInChannel(ulong userId, ulong channelId)
        {
            _userId = userId;
            _channelId = channelId;
        }
        
        public Task<bool> JudgeAsync(SocketCommandContext sourceContext, SocketMessage msg)
        {
            return Task.FromResult(msg.Author.Id == _userId && msg.Channel.Id == _channelId);
        }
    }
}