using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SoraBot.Common.Messages
{
    public class MessageBroker : IMessageBroker
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<MessageBroker> _log;

        public MessageBroker(IServiceScopeFactory serviceScopeFactory, ILogger<MessageBroker> log)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _log = log;
        }

        public void Dispatch<TMessage>(TMessage message) where TMessage : notnull, IMessage
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            // We intentionally don't await this call to force it to execute asynchronously and non-blocking
#pragma warning disable 4014
            DispatchAsync(message);
#pragma warning restore 4014
        }

        internal async Task DispatchAsync<TMessage>(TMessage message) where TMessage : notnull, IMessage
        {
            // Gotta try and catch all errors here because otherwise it will be swallowed since
            // this Task is not awaited. 
            try
            {
                using var serviceScope = _serviceScopeFactory.CreateScope();
                var messageHandlers = serviceScope.ServiceProvider.GetServices<IMessageHandler<TMessage>>();
                foreach (var handler in messageHandlers)
                {
                    try
                    {
                        await handler.HandleMessageAsync(message).ConfigureAwait(false);
                    }
                    catch (Exception e)
                    {
                        if (e.Message.Contains("Missing Permissions"))
                        {
                            _log.LogInformation($"Missing permissions to execute message {message.ToString()}");
                            return; // We don't care about missing perm exceptions.
                        }
                        
                        _log.LogError(e, "An unexpect exception occured while handling a dispatched message: {Message}",
                            message);
                    }
                }
            }
            catch (Exception e)
            {
                _log.LogError(e, "An unexpected exception occured while Dispatching a message: {Message}", message);
            }
        }
    }
}