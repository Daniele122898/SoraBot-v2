namespace SoraBot.Common.Notifications
{
    /// <summary>
    /// Any object that implements this interface is a notification that was or is being dispatched
    /// by the <see cref="IMessageBroker"/> and received and handled by one or multiple <see cref="IMessageHandler"/>
    /// </summary>
    public interface IMessage
    {
        
    }
}