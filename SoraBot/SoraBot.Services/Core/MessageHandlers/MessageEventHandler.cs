using System.Threading;
using System.Threading.Tasks;
using Discord.WebSocket;
using SoraBot.Common.Messages;
using SoraBot.Common.Messages.MessageAdapters;
using SoraBot.Services.Afk;
using SoraBot.Services.Profile;

namespace SoraBot.Services.Core.MessageHandlers
{    
    /// <summary>
    /// This handler is different from the <see cref="MessageReceivedHandler"/> bcs it does not handle direct commands.
    /// It handles stuff like AFK messages or gaining EXP.
    /// </summary>
    public class MessageEventHandler : IMessageHandler<MessageReceived>
    {
        private readonly IExpService _expService;
        private readonly IAfkService _afkService;

        public MessageEventHandler(IExpService expService, IAfkService afkService)
        {
            _expService = expService;
            _afkService = afkService;
        }
        
        public Task HandleMessageAsync(MessageReceived message, CancellationToken cancellationToken = default)
        {
            var msg = message.Message;
            if (msg.Author.IsBot || msg.Author.IsWebhook) return Task.CompletedTask;
            // Make sure we only respond to guild channels.
            if (!(msg.Channel is SocketGuildChannel channel))
                return Task.CompletedTask;
            
            // Now let's give them EXP
            var expTask = _expService.TryGiveUserExp(msg, channel);
            // Check AFK status
            var afkTask = _afkService.CheckUserAfkStatus(channel, msg.Author);
            Task.WaitAll(expTask, afkTask);
            return Task.CompletedTask;
        }
    }
}