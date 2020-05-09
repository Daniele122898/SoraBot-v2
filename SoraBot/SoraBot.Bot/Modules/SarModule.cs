﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using SoraBot.Common.Extensions.Modules;

namespace SoraBot.Bot.Modules
{
    
    [Name("SARs")]
    [Summary("All commands that have to do with Self-assignable roles")]
    public class SarModule : SoraSocketCommandModule
    {
        [Command("addsar"), Alias("asar", "addrole")]
        public async Task AddSar(string roleName)
        {
            if (!await this.UserHasGuildPermission(GuildPermission.Administrator))
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
            var soraHighestRole = Context.Guild.CurrentUser.Roles
                .OrderByDescending(x => x.Position)
                .FirstOrDefault();
            if (soraHighestRole == null || soraHighestRole.Position < role.Position)
            {
                await ReplyFailureEmbedExtended(
                    "I cannot assing the specified role!",
                    "For Sora to be able to assign this role he has to be or have a role that is above the specified role in the role hierarchy! " +
                    "Discord sometimes fails to update this order so just move a group around and it should be fine :)");
                return;
            }
        }
    }
}