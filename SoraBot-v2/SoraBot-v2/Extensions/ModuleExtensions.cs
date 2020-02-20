using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using SoraBot_v2.Services;

namespace SoraBot_v2.Extensions
{
    public static class ModuleExtensions
    {
        public static async Task<IUserMessage> ReplySoraEmbedResponse(this ModuleBase<SocketCommandContext> module, Color embedColor, string embedEmoji, string message)
        {
            return await module.Context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(embedColor, embedEmoji, message).Build());
        }
    }
}