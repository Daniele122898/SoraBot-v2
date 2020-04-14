using Discord.WebSocket;

namespace SoraBot.Common.Messages.MessageAdapters
{
    public class MessageReceived : IMessage
    {
        public SocketMessage Message { get; }

        public MessageReceived(SocketMessage message)
        {
            Message = message;
        }
    }
}