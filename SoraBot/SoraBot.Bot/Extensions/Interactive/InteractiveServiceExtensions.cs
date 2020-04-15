using System;
using Discord.Addons.Interactive;
using Discord.WebSocket;

namespace SoraBot.Bot.Extensions.Interactive
{
    public static class InteractiveServiceExtensions
    {
        public static Criteria<SocketMessage> CreateEnsureFromUserInChannelCriteria(ulong userId, ulong channelId)
        {
            Criteria<SocketMessage> criteria = new Criteria<SocketMessage>();
            criteria.AddCriterion(new EnsureFromUserInChannel(userId, channelId));
            return criteria;
        }

        public static bool StringIsYOrYes(string str)
            => str.Equals("y", StringComparison.OrdinalIgnoreCase) ||
               str.Equals("yes", StringComparison.OrdinalIgnoreCase);
        
        public static bool StringIsYes(string str)
            => str.Equals("yes", StringComparison.OrdinalIgnoreCase);

        public static bool StringContainsYes(string str)
            => str.Contains("yes", StringComparison.OrdinalIgnoreCase);

    }
}