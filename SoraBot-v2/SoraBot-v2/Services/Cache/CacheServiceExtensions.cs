using System;
using System.Threading.Tasks;
using Discord;

namespace SoraBot_v2.Services
{
    public static partial class CacheService
    {
        public static void SetGuildPrefix(ulong guildId, string prefix)
        {
            Set(DISCORD_GUILD_PREFIX + guildId.ToString(), new Item(prefix, DateTime.UtcNow.AddDays(30)));
        }

        public static string GetGuildPrefix(ulong guildId)
        {
            return _cacheDict.TryGetValue(DISCORD_GUILD_PREFIX + guildId.ToString(), out var item)
                ? (string)item.Content : null;
        }

        public static async Task<IUserMessage> GetUserMessage(ulong id, RequestOptions options = null)
        {
            string sId = DISCORD_USER_MESSAGE + id;
            object msgObj = Get(sId);
            IUserMessage msg = null;
            if (msgObj == null && options != null)
            {
                //Mesage isn't cached so check if request options are valid
                if (options.GetFrom is ITextChannel channel)
                {
                    //If the message isn't cachged get it (if Request options are given)
                    msg = await SetDiscordUserMessage(channel, id, options.Timeout);
                }
            }
            else if (msgObj != null)
            {
                msg = msgObj as IUserMessage;
            }
            return msg;
        }

        public static bool RemoveUserMessage(ulong id)
        {
            return _cacheDict.TryRemove(DISCORD_USER_MESSAGE + id, out _);
        }

        public static async Task<IUserMessage> SetDiscordUserMessage(ITextChannel channel, ulong msgId, TimeSpan timeout)
        {
            var msg = await channel.GetMessageAsync(msgId) as IUserMessage;
            if (msg == null)
                return null;
            Set(DISCORD_USER_MESSAGE + msgId, new Item(msg, DateTime.UtcNow.Add(timeout)));
            return msg;
        }

        public static async Task<IUserMessage> TrySetDiscordUserMessage(ITextChannel channel, ulong msgId,
            TimeSpan timeout)
        {
            var msgobj = Get(DISCORD_USER_MESSAGE + msgId);
            //MESSAGE ISNT CACHED! cache it
            if (msgobj == null)
            {
                return (await SetDiscordUserMessage(channel, msgId, timeout));
            }
            return (msgobj as IUserMessage);
        }
    }
}