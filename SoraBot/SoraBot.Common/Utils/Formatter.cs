using System;
using Discord;

namespace SoraBot.Common.Utils
{
    public static class Formatter
    {
        public static string UsernameDiscrim(IUser user) =>
            user == null ? "User Unknown" : $"{user.Username}#{user.Discriminator}";
        
        public static string FormatTime(in TimeSpan duration)
        {
            if (duration.TotalHours >= 1)
            {
                return duration.ToString(@"hh\:mm\:ss");
            }
            return duration.ToString(@"mm\:ss");
        }
    }
}