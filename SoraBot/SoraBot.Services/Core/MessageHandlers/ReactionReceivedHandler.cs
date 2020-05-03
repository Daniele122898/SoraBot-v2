using System;
using System.Threading;
using System.Threading.Tasks;
using SoraBot.Common.Messages;
using SoraBot.Common.Messages.MessageAdapters;
using SoraBot.Services.ReactionHandlers;

namespace SoraBot.Services.Core.MessageHandlers
{
    public class ReactionReceivedHandler : IMessageHandler<ReactionReceived>
    {
        private readonly IStarboardService _starboardService;

        public ReactionReceivedHandler(
            IStarboardService starboardService)
        {
            _starboardService = starboardService;
        }

        public async Task HandleMessageAsync(ReactionReceived reaction, CancellationToken cancellationToken = default)
        {
            switch (reaction.Type)
            {
                case ReactionEventType.Added:
                    await _starboardService.HandleReactionAdded(reaction.Message, reaction.Reaction)
                        .ConfigureAwait(false);
                    break;
                case ReactionEventType.Removed:
                    await _starboardService.HandleReactionRemoved(reaction.Message, reaction.Reaction)
                        .ConfigureAwait(false);
                    break;
                case ReactionEventType.Cleared:
                    await _starboardService.HandleReactionCleared(reaction.Message)
                        .ConfigureAwait(false);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}