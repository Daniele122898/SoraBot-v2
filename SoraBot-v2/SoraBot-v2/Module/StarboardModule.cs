using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Remotion.Linq.Clauses.ResultOperators;
using SoraBot_v2.Data;
using SoraBot_v2.Services;

namespace SoraBot_v2.Module
{
    public class StarboardModule : ModuleBase<SocketCommandContext>
    {
        private SoraContext _soraContext;

        public StarboardModule(SoraContext soraContext)
        {
            _soraContext = soraContext;
        }

        [Command("starchannel"), Alias("star"), Summary("Sets current or specified channel as starboard channel!")]
        public async Task AddStarChannel([Summary("Channel for starboard. Leave blank to use current channel")] ISocketMessageChannel channel = null)
        {
            var starChannel = channel ?? Context.Channel;
            var invoker = Context.User as SocketGuildUser;
            if (!invoker.GuildPermissions.Has(GuildPermission.Administrator) &&
                !invoker.GuildPermissions.Has(GuildPermission.ManageChannels))
            {
                await ReplyAsync("", embed: Utility.ResultFeedback(Utility.RedFailiureEmbed,
                    Utility.SuccessLevelEmoji[2], "You need Administrator or Mange Channels permission to set the starboard channel!"));
                return;
            }

            var guildDb = Utility.GetOrCreateGuild(Context.Guild, _soraContext);
            guildDb.StarChannelId = starChannel.Id;
            await _soraContext.SaveChangesAsync();
            await ReplyAsync("", embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed,
                Utility.SuccessLevelEmoji[0], "Successfully set starboard channel").WithDescription($"<#{starChannel.Id}>"));
        }
    }
}