using System;
using Discord;
using Discord.WebSocket;

namespace SoraBot.Common.Messages.MessageAdapters
{
    public enum ReactionEventType
    {
        Added,
        Removed,
        Cleared
    }
    
    public class ReactionReceived : IMessage
    {
        public Cacheable<IUserMessage, ulong> Message { get; private set; }
        public ISocketMessageChannel Channel { get; private set; }
        public SocketReaction Reaction { get; private set; }
        public ReactionEventType Type { get; private set; }

        public ReactionReceived(
            ReactionEventType type, 
            Cacheable<IUserMessage, ulong> message, 
            ISocketMessageChannel channel,
            SocketReaction reaction = null)
        {
            this.Type = type;
            this.Message = message;
            this.Channel = channel;
            if (type != ReactionEventType.Cleared && reaction == null)
                throw new ArgumentNullException(nameof(reaction));
            this.Reaction = reaction;
        }
    }
}