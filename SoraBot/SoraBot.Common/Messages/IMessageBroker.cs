using Microsoft.Extensions.DependencyInjection;

namespace SoraBot.Common.Messages
{
    /// <summary>
    /// Object that dispatches messages to be handled by <see cref="IMessageHandler{TMessage}"/>
    /// </summary>
    public interface IMessageBroker
    {
        /// <summary>
        /// Dispatches a message to be handled by the <see cref="IMessageHandler{TMessage}"/> with the
        /// correct type. This dispatch will happen on a new Task that is NOT awaited and thus runs in a
        /// separate thread on the thread pool. This makes it possible to not block the GateWay thread.
        /// This will also create a new <see cref="IServiceScope"/> for the command to be executed so
        /// we dont have to do it in the <see cref="IMessageHandler{TMessage}"/>.
        /// </summary>
        /// <param name="message">Type of message to dispatch</param>
        /// <typeparam name="TMessage">The actual message to dispatch</typeparam>
        void Dispatch<TMessage>(TMessage message) where TMessage : notnull, IMessage;
    }
}