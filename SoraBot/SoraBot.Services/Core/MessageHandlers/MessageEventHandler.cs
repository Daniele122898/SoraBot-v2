using System.Threading;
using System.Threading.Tasks;
using Discord.WebSocket;
using SoraBot.Common.Messages;
using SoraBot.Common.Messages.MessageAdapters;

namespace SoraBot.Services.Core.MessageHandlers
{    
    /// <summary>
    /// This handler is different from the <see cref="MessageReceivedHandler"/> bcs it does not handle direct commands.
    /// It handles stuff like AFK messages or gaining EXP.
    /// </summary>
    public class MessageEventHandler : IMessageHandler<MessageReceived>
    {
        public async Task HandleMessageAsync(MessageReceived message, CancellationToken cancellationToken = default)
        {
            var msg = message.Message;
            if (msg.Author.IsBot || msg.Author.IsWebhook) return;
            // Make sure we only respond to guild channels.
            if (!(msg.Channel is SocketGuildChannel channel))
                return;
            
            // Now let's give them EXP
        }
    }
}