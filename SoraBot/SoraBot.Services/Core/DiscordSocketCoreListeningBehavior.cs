using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SoraBot.Common.Extensions.Hosting;
using SoraBot.Common.Messages;
using SoraBot.Common.Messages.MessageAdapters;
using SoraBot.Data.Repositories.Interfaces;
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
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DiscordSocketCoreListeningBehavior> _log;

        public DiscordSocketCoreListeningBehavior(
            DiscordSocketClient client,
            IMessageBroker broker,
            ICacheService cacheService,
            IServiceScopeFactory scopeFactory,
            ILogger<DiscordSocketCoreListeningBehavior> log)
        {
            _client = client;
            _broker = broker;
            _cacheService = cacheService;
            _scopeFactory = scopeFactory;
            _log = log;
        }
        
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _client.MessageReceived += OnMessageReceivedAsnyc;
            _client.ReactionAdded += OnReactionAdded;
            _client.ReactionRemoved += OnReactionRemoved;
            _client.ReactionsCleared += OnReactionsCleared;
            _client.MessageDeleted += OnMessageDeleted;
            _client.UserLeft += OnUserLeft;
            _client.LeftGuild += OnLeftGuild;

            return Task.CompletedTask;
        }

        private async Task OnLeftGuild(SocketGuild guild)
        {
            var sw = new Stopwatch();
            sw.Start();
            using var scope = _scopeFactory.CreateScope();
            // Alright so we just remove everything that has to do with the guild. 
            // For this we don't rly need a separate thread i believe so we're gonna do it on the GW thread.
            var guildRepo = scope.ServiceProvider.GetRequiredService<IGuildRepository>();
            await guildRepo.RemoveGuild(guild.Id).ConfigureAwait(false);
            sw.Stop();
            _log.LogInformation($"Removing Guild from DB in {sw.ElapsedMilliseconds.ToString()} ms.");
            // Clear the cache for prefix etc
            _cacheService.TryRemove(CacheID.PrefixCacheId(guild.Id));
        }

        private Task OnUserLeft(SocketGuildUser user)
        {
            // When a user leaves a guild there's a possibility that Sora lost reach of him.
            // Thus we just clear him out of the cache
            _cacheService.TryRemove(user.Id);
            return Task.CompletedTask;
        }

        private Task OnMessageDeleted(Cacheable<IMessage, ulong> message, ISocketMessageChannel channel)
        {
            // There's no need to dispatch an event yet internally so no need for async state machines
            // and threading. That's just overhead we dont need atm for just cleaning some caches.
            _cacheService.TryRemove(message.Id);
            _cacheService.TryRemove(CacheID.StarboardDoNotPostId(message.Id));
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