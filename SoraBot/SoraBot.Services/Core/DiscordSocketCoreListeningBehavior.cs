using System.Threading;
using System.Threading.Tasks;
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