using System.Threading;
using System.Threading.Tasks;

namespace SoraBot.Common.Messages
{
    /// <summary>
    /// Interface for object that handles messages published by an <see cref="IMessageBroker"/>
    /// </summary>
    /// <typeparam name="TMessage">Type of notification this object handles</typeparam>
    public interface IMessageHandler<TMessage> where TMessage : IMessage
    {
        /// <summary>
        /// Handles a published message
        /// </summary>
        /// <param name="message">Message to be handled</param>
        /// <param name="cancellationToken">Cancellation token to cancel async operations</param>
        /// <returns>Task indicating success</returns>
        Task HandleMessageAsync(TMessage message, CancellationToken cancellationToken = default);
    }
}