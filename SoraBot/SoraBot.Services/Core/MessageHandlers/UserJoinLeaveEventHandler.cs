using System.Threading;
using System.Threading.Tasks;
using SoraBot.Common.Messages;
using SoraBot.Common.Messages.MessageAdapters;

namespace SoraBot.Services.Core.MessageHandlers
{
    public class UserJoinLeaveEventHandler : IMessageHandler<UserJoined>, IMessageHandler<UserLeft>
    {
        public Task HandleMessageAsync(UserJoined message, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }

        public Task HandleMessageAsync(UserLeft message, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }
    }
}