using Discord;

namespace SoraBot.Common.Utils
{
    public static class Formatter
    {
        public static string UsernameDiscrim(IUser user) =>
            user == null ? "User Unknown" : $"{user.Username}#{user.Discriminator}";
    }
}