using Discord.WebSocket;

namespace SoraBot.Common.Messages.MessageAdapters
{
    public class UserJoined : IMessage
    {
        public readonly SocketGuildUser GuildUser;

        public UserJoined(SocketGuildUser guildUser)
        {
            this.GuildUser = guildUser;
        }
    }
}