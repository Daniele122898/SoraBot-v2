using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using SoraBot.Common.Extensions.Modules;
using SoraBot.Data.Repositories.Interfaces;

namespace SoraBot.Bot.Modules
{
    
    [Name("SARs")]
    [Summary("All commands that have to do with Self-assignable roles")]
    public class SarModule : SoraSocketCommandModule
    {
        private readonly ISarRepository _sarRepo;
        private readonly ILogger<SarModule> _log;

        public SarModule(ISarRepository sarRepo, ILogger<SarModule> log)
        {
            _sarRepo = sarRepo;
            _log = log;
        }

        [Command("sarlist"), Alias("sars")]
        [Summary("Lists all the Self assignable roles in this guild")]
        public async Task Sars()
        {
            var sars = await _sarRepo.GetAllSarsInGuild(Context.Guild.Id);
            if (!sars.HasValue)
            {
                await ReplyFailureEmbed("This guild has no self assignable roles!");
                return;
            }
            var ss = sars.Value;
            
            var eb = new EmbedBuilder()
            {
                Color = Purple,
                ThumbnailUrl = Context.Guild.IconUrl ?? Context.Client.CurrentUser.GetAvatarUrl(),
                Footer = RequestedByMe(),
                Title = $"All available Self-Assignable roles in {Context.Guild.Name}"
            };

            var roles = ss
                .Select(x =>
                {
                    var role = Context.Guild.GetRole(x.RoleId);
                    return role == null ? null : $"- {role.Name}";
                })
                .Where(x => x != null);
            var desc = String.Join("\n", roles);
            eb.WithDescription(desc);
            
            await ReplyEmbed(eb);
        }
        
        [Command("iamnot")]
        [Summary("Removes the specified role from your roles if it is a self assignable role")]
        public async Task IamNot(
            [Summary("Name of the role"), Remainder]
            string roleName)
        {
            if (!await this.SoraHasManageRolesPerm())
                return;
            
            // Try to find the role specified
            var role = Context.Guild.Roles
                .FirstOrDefault(x => x.Name.Equals(roleName, StringComparison.InvariantCultureIgnoreCase));
            if (role == null)
            {
                await ReplyFailureEmbed("Could not find role! Make sure the role exists and is correctly spelled!");
                return;
            }
            
            // Make sure Sora could even assign it
            if (!await this.SoraCanAssignRole(
                role.Position, 
                "I cannot remove the specified role!",
                "For Sora to be able to remove this role he has to be - or have a role that is - above the specified role in the role hierarchy! "))
                return;
            
            // Check if user even has it
            var user = (SocketGuildUser) Context.User;
            if (user.Roles.All(x => x.Id != role.Id))
            {
                await ReplyFailureEmbed("You don't have this role!");
                return;
            }
            
            // Check if it's even a SAR
            if (!await _sarRepo.CheckIfRoleAlreadyExists(role.Id))
            {
                await ReplyFailureEmbed("This role is not a self assignable role.");
                return;
            }
            
            // Otherwise remove it from the user
            if (await user.TryRemoveRoleAsync(role, _log))
            {
                await ReplySuccessEmbed($"Successfully removed {role.Name} from you :)");
            }
            else
            {
                await ReplyFailureEmbedExtended("Failed to removed the role from you.",
                    "This could have different causes. Maybe the user is a guest user for which no roles can be assigned or removed by a bot. " +
                    "Could also be a permission error etc. Another account should try to the command and see if it works.");
            }
        }

        [Command("iam"), Alias("sar")]
        [Summary("Adds the specified role to your roles if it is a self assignable role")]
        public async Task Iam(
            [Summary("Name of the role"), Remainder]
            string roleName)
        {
            if (!await this.SoraHasManageRolesPerm())
                return;
            
            // Try to find the role specified
            var role = Context.Guild.Roles
                .FirstOrDefault(x => x.Name.Equals(roleName, StringComparison.InvariantCultureIgnoreCase));
            if (role == null)
            {
                await ReplyFailureEmbed("Could not find role! Make sure the role exists and is correctly spelled!");
                return;
            }
            
            // Make sure Sora could even assign it
            if (!await this.SoraCanAssignRole(
                role.Position, 
                "I cannot assing the specified role!",
                "For Sora to be able to assign this role he has to be - or have a role that is - above the specified role in the role hierarchy! "))
                return;
            
            // Check if user already has it
            var user = (SocketGuildUser) Context.User;
            if (user.Roles.Any(x => x.Id == role.Id))
            {
                await ReplyFailureEmbed("You already have this role!");
                return;
            }
            
            // Check if it's even a SAR
            if (!await _sarRepo.CheckIfRoleAlreadyExists(role.Id))
            {
                await ReplyFailureEmbed("This role is not a self assignable role.");
                return;
            }
            
            // Otherwise give it to the user
            if (await user.TryAddRoleAsync(role, _log))
            {
                await ReplySuccessEmbed($"Successfully assigned {role.Name} to you :)");
            }
            else
            {
                await ReplyFailureEmbedExtended("Failed to assign the role.",
                    "This could have different causes. Maybe the user is a guest user for which no roles can be assigned by a bot. " +
                    "Could also be a permission error etc. Another account should try to the command and see if it works.");
            }
        }
        
        [Command("addsar"), Alias("asar", "addrole")]
        public async Task AddSar(
            [Summary("Name of the role"), Remainder]
            string roleName)
        {
            if (!await this.UserHasGuildPermission(GuildPermission.Administrator))
                return;
            
            if (!await this.SoraHasManageRolesPerm())
                return;
            
            // Try to find the role specified
            var role = Context.Guild.Roles
                .FirstOrDefault(x => x.Name.Equals(roleName, StringComparison.InvariantCultureIgnoreCase));
            if (role == null)
            {
                await ReplyFailureEmbed("Could not find role! Make sure the role exists and is correctly spelled!");
                return;
            }
            
            // Make sure that sora is ABOVE the role in the hierarchy. Otherwise he cannot assign the role
            if (!await this.SoraCanAssignRole(
                role.Position, 
                "I cannot assing the specified role!",
                "For Sora to be able to assign this role he has to be - or have a role that is - above the specified role in the role hierarchy! " +
                "Discord sometimes fails to update this order so just move a group around and it should be fine :)"))
                return;

            // Now make sure it doesnt already exist
            if (await _sarRepo.CheckIfRoleAlreadyExists(role.Id))
            {
                await ReplyFailureEmbed("This role is already self assignable.");
                return;
            }
            
            // Role exists, Sora can assign it, and it's not a sar yet. So create it :D
            await _sarRepo.AddSar(role.Id, Context.Guild.Id);
            await ReplySuccessEmbed($"Successfully added {role.Name} to the SAR list :>");
        }

        [Command("rmsar"), Alias("rsar", "rmrole", "delrole")]
        [Summary("Removes the specified role from the SAR list")]
        public async Task RemoveSar(
            [Summary("Name of the role"), Remainder]
            string roleName)
        {
            if (!await this.UserHasGuildPermission(GuildPermission.Administrator))
                return;

            if (!await this.SoraHasManageRolesPerm())
                return;
            
            // Try to find the role specified
            var role = Context.Guild.Roles
                .FirstOrDefault(x => x.Name.Equals(roleName, StringComparison.InvariantCultureIgnoreCase));
            if (role == null)
            {
                await ReplyFailureEmbed("Could not find role! Make sure the role exists and is correctly spelled!");
                return;
            }
            
            // Now make sure it exists
            if (!await _sarRepo.CheckIfRoleAlreadyExists(role.Id))
            {
                await ReplyFailureEmbed("This role is not a self assignable role.");
                return;
            }
            
            // Role exists, Sora can assign it, and it's not a sar yet. So create it :D
            await _sarRepo.RemoveSar(role.Id);
            await ReplySuccessEmbed($"Successfully removed {role.Name} from the SAR list :>");
        }

        private async Task<bool> SoraHasManageRolesPerm()
        {
            if (Context.Guild.CurrentUser.GuildPermissions.Has(GuildPermission.ManageRoles))
                return true;

            await ReplyFailureEmbed("I need Manage Roles permission to perform self assignable role commands >.<");
            return false;
        }
        
        private async Task<bool> SoraCanAssignRole(int rolePosition, string failiureTitle, string failiureMsg)
        {
            if (Context.Guild.CurrentUser.Hierarchy < rolePosition)
            {
                await ReplyFailureEmbedExtended(failiureTitle, failiureMsg);
                return false;
            }
            return true;
        }
    }
}