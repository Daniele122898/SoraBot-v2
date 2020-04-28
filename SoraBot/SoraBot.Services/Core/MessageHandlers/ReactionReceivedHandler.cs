using System;
using System.Threading;
using System.Threading.Tasks;
using SoraBot.Common.Messages;
using SoraBot.Common.Messages.MessageAdapters;

namespace SoraBot.Services.Core.MessageHandlers
{
    public class ReactionReceivedHandler : IMessageHandler<ReactionReceived>
    {
        public async Task HandleMessageAsync(ReactionReceived reaction, CancellationToken cancellationToken = default)
        {
            switch (reaction.Type)
            {
                case ReactionEventType.Added:
                    break;
                case ReactionEventType.Removed:
                    break;
                case ReactionEventType.Cleared:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}