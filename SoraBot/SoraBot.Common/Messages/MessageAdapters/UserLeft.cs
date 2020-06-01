using Discord.WebSocket;

namespace SoraBot.Common.Messages.MessageAdapters
{
    public class UserLeft : IMessage
    {
        public readonly SocketGuildUser GuildUser;

        public UserLeft(SocketGuildUser guildUser)
        {
            this.GuildUser = guildUser;
        }
    }
}