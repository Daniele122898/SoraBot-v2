using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using SoraBot.Common.Extensions.Hosting;
using SoraBot.Common.Messages;
using SoraBot.Common.Messages.MessageAdapters;
using SoraBot.Services.Cache;
using SoraBot.Services.ReactionHandlers;
using IMessage = Discord.IMessage;

namespace SoraBot.Services.Core
{
    public class DiscordSocketCoreListeningBehavior : IBehavior
    {
        private readonly DiscordSocketClient _client;
        private readonly IMessageBroker _broker;
        private readonly ICacheService _cacheService;

        public DiscordSocketCoreListeningBehavior(
            DiscordSocketClient client,
            IMessageBroker broker,
            ICacheService cacheService)
        {
            _client = client;
            _broker = broker;
            _cacheService = cacheService;
        }
        
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _client.MessageReceived += OnMessageReceivedAsnyc;
            _client.ReactionAdded += OnReactionAdded;
            _client.ReactionRemoved += OnReactionRemoved;
            _client.ReactionsCleared += OnReactionsCleared;
            _client.MessageDeleted += OnMessageDeleted;

            return Task.CompletedTask;
        }

        private Task OnMessageDeleted(Cacheable<IMessage, ulong> message, ISocketMessageChannel channel)
        {
            // There's no need to dispatch an event yet internally so no need for async state machines
            // and threading. That's just overhead we dont need atm for just cleaning some caches.
            _cacheService.TryRemove(message.Id);
            _cacheService.TryRemove(StarboardService.DoNotPostId(message.Id));
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
            _client.ReactionAdded -= OnReactionAdded;
            _client.ReactionRemoved -= OnReactionRemoved;
            _client.ReactionsCleared -= OnReactionsCleared;
            _client.MessageDeleted -= OnMessageDeleted;

            return Task.CompletedTask;
        }
    }
}