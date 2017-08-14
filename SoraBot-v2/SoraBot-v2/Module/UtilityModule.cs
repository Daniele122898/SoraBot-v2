using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using SoraBot_v2.Services;

namespace SoraBot_v2.Module
{
    public class UtilityModule : ModuleBase<SocketCommandContext>
    {
        [Command("createAdmin"), Alias("ca", "createsoraadmin", "csa"), Summary("Creates the Admin Role for Sora!")]
        public async Task CreateSoraAdminRole()
        {
            var invoker = Context.User as SocketGuildUser;
            if (!invoker.GuildPermissions.Has(GuildPermission.Administrator))
            {
                await ReplyAsync("", embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"You need Administrator permissions to create the {Utility.SORA_ADMIN_ROLE_NAME}"));
                return;
            }
            if (!Context.Guild.GetUser(Context.Client.CurrentUser.Id).GuildPermissions.Has(GuildPermission.ManageRoles))
            {
                await ReplyAsync("", embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"Sora does not have Manage Role Permissions!"));
                return;
            }
            
            //Check if already exists
            if (Utility.CheckIfSoraAdminExists(Context.Guild))
            {
                await ReplyAsync("", embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"The {Utility.SORA_ADMIN_ROLE_NAME} Role already exists!"));
                return;
            }
            //Create role
            await Context.Guild.CreateRoleAsync(Utility.SORA_ADMIN_ROLE_NAME, GuildPermissions.None);
            await ReplyAsync("",
                embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0],
                    $"Successfully created {Utility.SORA_ADMIN_ROLE_NAME} Role!"));
        }
    }
}