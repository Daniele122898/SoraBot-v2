using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using SoraBot.Common.Extensions.Hosting;
using SoraBot.Common.Messages;
using SoraBot.Common.Messages.MessageAdapters;

namespace SoraBot.Services.Core
{
    public class DiscordSocketCoreListeningBehavior : IBehavior
    {
        private readonly DiscordSocketClient _client;
        private readonly IMessageBroker _broker;

        public DiscordSocketCoreListeningBehavior(
            DiscordSocketClient client,
            IMessageBroker broker)
        {
            _client = client;
            _broker = broker;
        }
        
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _client.MessageReceived += OnMessageReceivedAsnyc;
            _client.ReactionAdded += OnReactionAdded;
            _client.ReactionRemoved += OnReactionRemoved;
            _client.ReactionsCleared += OnReactionsCleared;

            return Task.CompletedTask;
        }

        private Task OnReactionsCleared(Cacheable<IUserMessage, ulong> msg, ISocketMessageChannel channel)
        {
            _broker.Dispatch(new ReactionReceived(ReactionEventType.Cleared, msg, channel));
            return Task.CompletedTask;
        }

        private Task OnReactionRemoved(Cacheable<IUserMessage, ulong> msg, ISocketMessageChannel channel, SocketReaction reaction)
        {
            _broker.Dispatch(new ReactionReceived(ReactionEventType.Removed, msg, channel, reaction));
            return Task.CompletedTask;
        }

        private Task OnReactionAdded(Cacheable<IUserMessage, ulong> msg, ISocketMessageChannel channel, SocketReaction reaction)
        {
            _broker.Dispatch(new ReactionReceived(ReactionEventType.Added, msg, channel, reaction));
            return Task.CompletedTask;
        }

        private Task OnMessageReceivedAsnyc(SocketMessage m)
        {
            _broker.Dispatch(new MessageReceived(m));

            return Task.CompletedTask;
        }
        
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _client.MessageReceived -= OnMessageReceivedAsnyc;

            return Task.CompletedTask;
        }
    }
}