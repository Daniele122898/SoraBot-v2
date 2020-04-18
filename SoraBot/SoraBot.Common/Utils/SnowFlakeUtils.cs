using System;
using Discord;

namespace SoraBot.Common.Utils
{
    public static class SnowFlakeUtils
    {
        // FROM MODIX BOT
        public static bool IsValidSnowflake(ulong snowflake)
        {
            // Jan 1, 2015
            var discordEpoch = SnowflakeUtils.FromSnowflake(0);

            // The supposed timestamp
            var snowflakeDateTime = SnowflakeUtils.FromSnowflake(snowflake);

            return snowflakeDateTime > discordEpoch
                   && snowflakeDateTime < DateTimeOffset.UtcNow;
        }
    }
}