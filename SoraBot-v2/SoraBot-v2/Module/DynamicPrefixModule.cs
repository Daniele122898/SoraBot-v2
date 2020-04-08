using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using SoraBot_v2.Data;
using SoraBot_v2.Services;

namespace SoraBot_v2.Module
{
    [Name("Misc")]
    public class DynamicPrefixModule : ModuleBase<SocketCommandContext>
    {

        [Command("prefix"), Summary("Changes the prefix of the bot for this guild")]
        public async Task ChangeGuildPrefix([Summary("Prefix to change to")] string prefix)
        {
            var user = ((SocketGuildUser) Context.User);
            if (!user.GuildPermissions.Has(GuildPermission.Administrator) && !Utility.IsSoraAdmin(user))
            {
                await ReplyAsync("",
                    embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            $"You don't have permission to set the prefix! You need Administrator permissions or the {Utility.SORA_ADMIN_ROLE_NAME} role!")
                        .Build());
                return;
            }

            if (string.IsNullOrWhiteSpace(prefix))
            {
                await ReplyAsync("",
                    embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                        "Prefix can't be null or whitespace!").Build());
                return;
            }

            using var soraContext = new SoraContext();
            prefix = prefix.Trim();
            var guildDb = Utility.GetOrCreateGuild(Context.Guild.Id, soraContext);
            guildDb.Prefix = prefix;
            await soraContext.SaveChangesAsync();
            // Set Cache
            CacheService.SetGuildPrefix(Context.Guild.Id, prefix);
            
            await ReplyAsync("",
                embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0],
                    $"Prefix in this Guild was changed to `{prefix}`").Build());
        }

        [Command("prefix"), Summary("Shows the current prefix for the Guild")]
        public async Task CheckPrefix()
        {
            string prefixCacheId = CacheService.DISCORD_GUILD_PREFIX + Context.Guild.Id.ToString();
            var prefix = await CacheService.GetOrSetAsync<string>(prefixCacheId, async () =>
            {
                using var soraContext = new SoraContext();
                var guild = await soraContext.Guilds.FindAsync(Context.Guild.Id);
                if (guild == null) return "$";

                return guild.Prefix;
            });

            await Context.Channel.SendMessageAsync("",
                embed: Utility.ResultFeedback(Utility.BlueInfoEmbed, Utility.SuccessLevelEmoji[3],
                    $"Prefix for this Guild is `{prefix}`").Build());
        }
    }
}