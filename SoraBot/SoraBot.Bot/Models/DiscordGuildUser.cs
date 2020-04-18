using Discord;

namespace SoraBot.Bot.Models
{
    public class DiscordGuildUser
    {
        public IGuildUser GuildUser { get; }

        public DiscordGuildUser(IGuildUser guildUser)
        {
            this.GuildUser = guildUser;
        }
    }
}