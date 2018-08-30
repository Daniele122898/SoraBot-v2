using System.ComponentModel;
using System.Threading.Tasks;
using Discord.Commands;
using SoraBot_v2.Data;

namespace SoraBot_v2.Services
{
    public class DynamicPrefixService
    {
        public async Task UpdateGuildPrefix(SocketCommandContext context, SoraContext soraContext, string prefix)
        {
            var guildDb = Utility.GetOrCreateGuild(context.Guild.Id, soraContext);
            guildDb.Prefix = prefix;
            await soraContext.SaveChangesAsync();
            await context.Channel.SendMessageAsync("",
                embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0],
                    $"Prefix in this Guild was changed to `{prefix}`").Build());
        }

        public async Task ReturnGuildPrefix(SocketCommandContext context, SoraContext soraContext)
        {
            var guildDb = Utility.GetOrCreateGuild(context.Guild.Id, soraContext);
            await context.Channel.SendMessageAsync("",
                embed: Utility.ResultFeedback(Utility.BlueInfoEmbed, Utility.SuccessLevelEmoji[3],
                    $"Prefix for this Guild is `{guildDb.Prefix}`").Build());
        }
    }
}