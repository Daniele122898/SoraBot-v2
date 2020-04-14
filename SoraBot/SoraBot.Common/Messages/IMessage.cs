namespace SoraBot.Common.Messages
{
    /// <summary>
    /// Any object that implements this interface is a notification that was or is being dispatched
    /// by the <see cref="IMessageBroker"/> and received and handled by one or multiple <see cref="IMessageHandler{TMessage}"/>
    /// </summary>
    public interface IMessage
    {
        
    }
}