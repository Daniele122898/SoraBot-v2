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
                    await this.HandleReactionAdded(reaction).ConfigureAwait(false);
                    break;
                case ReactionEventType.Removed:
                    await this.HandleReactionRemoved(reaction).ConfigureAwait(false);
                    break;
                case ReactionEventType.Cleared:
                    await this.HandleReactionCleared(reaction).ConfigureAwait(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private async Task HandleReactionAdded(ReactionReceived reaction)
        {
            throw new NotImplementedException();
        }

        private async Task HandleReactionRemoved(ReactionReceived reaction)
        {
            throw new NotImplementedException();
        }

        private async Task HandleReactionCleared(ReactionReceived reaction)
        {
            throw new NotImplementedException();
        }
    }
}