using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using SoraBot_v2.Services;

namespace SoraBot_v2.Extensions
{
    public static class ModuleExtensions
    {
        // Module extensions
        public static async Task<IUserMessage> ReplySoraEmbedResponse(this ModuleBase<SocketCommandContext> module, Color embedColor, string embedEmoji, string message)
        {
            return await module.Context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(embedColor, embedEmoji, message).Build());
        }
        
        public static async Task<IUserMessage> ReplySoraEmbedFailiureResponse(this ModuleBase<SocketCommandContext> module, string message)
        {
            return await module.Context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.FailiureEmoji, message).Build());
        }
        
        public static async Task<IUserMessage> ReplySoraEmbedWarningResponse(this ModuleBase<SocketCommandContext> module, string message)
        {
            return await module.Context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(Utility.YellowWarningEmbed, Utility.WarnEmoji, message).Build());
        }
        
        public static async Task<IUserMessage> ReplySoraEmbedSuccessResponse(this ModuleBase<SocketCommandContext> module, string message)
        {
            return await module.Context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessEmoji, message).Build());
        }
        
        // CommandSocket Extensions
        public static async Task<IUserMessage> ReplySoraEmbedResponse(this SocketCommandContext context, Color embedColor, string embedEmoji, string message)
        {
            return await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(embedColor, embedEmoji, message).Build());
        }
        
        public static async Task<IUserMessage> ReplySoraEmbedFailiureResponse(this SocketCommandContext context, string message)
        {
            return await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.FailiureEmoji, message).Build());
        }
        
        public static async Task<IUserMessage> ReplySoraEmbedWarningResponse(this SocketCommandContext context, string message)
        {
            return await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(Utility.YellowWarningEmbed, Utility.WarnEmoji, message).Build());
        }
        
        public static async Task<IUserMessage> ReplySoraEmbedSuccessResponse(this SocketCommandContext context, string message)
        {
            return await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessEmoji, message).Build());
        }
    }
}