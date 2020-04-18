using Discord;

namespace SoraBot.Bot.Models
{
    public class DiscordUser
    {
        public IUser User { get; }

        public DiscordUser(IUser user)
        {
            this.User = user;
        }
    }
}