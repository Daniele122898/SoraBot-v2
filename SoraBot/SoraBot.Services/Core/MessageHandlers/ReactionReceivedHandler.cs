using System.Threading;
using System.Threading.Tasks;
using SoraBot.Common.Messages;
using SoraBot.Common.Messages.MessageAdapters;

namespace SoraBot.Services.Core.MessageHandlers
{
    public class ReactionReceivedHandler : IMessageHandler<ReactionReceived>
    {
        public Task HandleMessageAsync(ReactionReceived message, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }
    }
}