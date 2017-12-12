using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using SoraBot_v2.Data;
using SoraBot_v2.Data.Entities.SubEntities;
using SoraBot_v2.Extensions;

namespace SoraBot_v2.Services
{
    public class SelfAssignableRolesService
    {
        private IServiceProvider _services;
        private InteractiveService _interactive;

        public SelfAssignableRolesService(InteractiveService service)
        {
            _interactive = service;
        }

        public void Initialize(IServiceProvider services)
        {
            _services = services;
        }


        public async Task ClientOnUserJoined(SocketGuildUser socketGuildUser)
        {
            var guild = socketGuildUser.Guild;
            using (SoraContext soraContext = new SoraContext())
            {
                var guildDb = Utility.GetOrCreateGuild(guild.Id, soraContext);

                //Check if default role is even on
                if (!guildDb.HasDefaultRole)
                    return;
                var sora = guild.CurrentUser;
                //Check if sora has manageRoles perms!
                if (!sora.GuildPermissions.Has(GuildPermission.ManageRoles))
                {
                    await (await guild.Owner.GetOrCreateDMChannelAsync()).SendMessageAsync("", embed: Utility.ResultFeedback(
                        Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Sora failed to add a default role!")
                    .WithDescription("He needs the ManageRole permissions to assign roles. The role must also be below his highest role!\n" +
                                     "Assigning default roles has been turned off. Fix the issue and then turn it back on by using the toggle command!"));
                    guildDb.HasDefaultRole = false;
                    await soraContext.SaveChangesAsync();
                    return;
                }
                //check if role still exists :P
                IRole role = guild.GetRole(guildDb.DefaultRoleId);
                //Role doesnt exist anymore, set defaultrole to false
                if (role == null)
                {
                    await (await guild.Owner.GetOrCreateDMChannelAsync()).SendMessageAsync("", embed: Utility.ResultFeedback(
                            Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Sora failed to add a default role!")
                        .WithDescription("The default role doesn't exist anymore.\n" +
                                         "Assigning default roles has been turned off. Fix the issue and then turn it back on by using the toggle command!"));
                    guildDb.HasDefaultRole = false;
                    await soraContext.SaveChangesAsync();
                    return;
                }
                //role exists, is set to true and he has perms. so assign it
                await socketGuildUser.AddRoleAsync(role);
            }
        }

        public async Task RemoveSarFromList(SocketCommandContext context, string roleName)
        {
            //check perms
            if (await Utility.HasAdminOrSoraAdmin(context) == false)
                return;
            var sora = context.Guild.CurrentUser;
            //Try to find role
            var role = context.Guild.Roles.FirstOrDefault(x =>
                x.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase));
            //Role wasn't found
            if (role == null)
            {
                await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                    Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                    "The Specified Role doesn't exist!"));
                return;
            }
            //role was found
            using (SoraContext soraContext = new SoraContext())
            {
                //Check if its self-assignable
                var guildDb = Utility.GetOrCreateGuild(context.Guild.Id, soraContext);
                var sarRole = guildDb.SelfAssignableRoles.FirstOrDefault(x => x.RoleId == role.Id);
                if (sarRole == null)
                {
                    await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                        Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                        "This role is not self-assignable!"));
                    return;
                }
                //Role is self assignable
                guildDb.SelfAssignableRoles.Remove(sarRole);
                await soraContext.SaveChangesAsync();
            }
            await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0],
                $"Successfully removed {role.Name} from self-assignable roles!"));
        }

        public async Task ToggleDefaultRole(SocketCommandContext context)
        {
            //check perms
            if (await Utility.HasAdminOrSoraAdmin(context) == false)
                return;
            using (SoraContext soraContext = new SoraContext())
            {
                var guildDb = Utility.GetOrCreateGuild(context.Guild.Id, soraContext);
                //if he plans to turn it on check if role exists
                if (!guildDb.HasDefaultRole)
                {
                    //check if role still exists :P
                    IRole role = context.Guild.GetRole(guildDb.DefaultRoleId);
                    //Role doesnt exist anymore
                    if (role == null)
                    {
                        await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                            Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            "The default role doesn't exist! Please set one before changing this to true."));
                        return;
                    }
                    var sora = context.Guild.CurrentUser;
                    //Check if sora has manage role perms
                    if (!sora.GuildPermissions.Has(GuildPermission.ManageRoles))
                    {
                        await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                            Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            "Sora needs ManageRole permissions for Default roles to work!"));
                        return;
                    }
                    //set it to true
                    guildDb.HasDefaultRole = true;
                    await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                        Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0],
                        "Default role is now activated!"));
                }
                else
                {
                    guildDb.HasDefaultRole = false;
                    await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                        Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0],
                        "Default role is now deactivated!"));
                }
                await soraContext.SaveChangesAsync();
            }
        }

        public async Task AddDefaultRole(SocketCommandContext context, string roleName)
        {
            //check perms
            if (await Utility.HasAdminOrSoraAdmin(context) == false)
                return;
            var sora = context.Guild.CurrentUser;
            //Check if sora has manage role perms
            if (!sora.GuildPermissions.Has(GuildPermission.ManageRoles))
            {
                await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                    Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                    "Sora needs ManageRole permissions for Default roles to work!"));
                return;
            }
            //Try to find role
            IRole role = context.Guild.Roles.FirstOrDefault(x =>
                x.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase));
            bool wasCreated = false;
            //Role wasn't found
            if (role == null)
            {
                //he has the perms so he can create the role
                role = await context.Guild.CreateRoleAsync(roleName, GuildPermissions.None);
                wasCreated = true;
            }
            else
            {
                //check if the role found is ABOVE sora if so.. quit. (on life)
                var soraHighestRole = sora.Roles.OrderByDescending(x => x.Position).FirstOrDefault();
                //Sora is below in the hirarchy
                if (soraHighestRole.Position < role.Position)
                {
                    await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                        Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "I cannot assign roles that are above me in the role hirachy!")
                        .WithDescription("If this is not the case, open the server settings and move a couple roles around since discord doesn't refresh the position unless they are moved."));
                    return;
                }
            }
            //role was either found or created by sora.. 
            using (SoraContext soraContext = new SoraContext())
            {
                //check if it already exists
                var guildDb = Utility.GetOrCreateGuild(context.Guild.Id, soraContext);
                if (guildDb.DefaultRoleId == role.Id)
                {
                    await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                        Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "This role already is the default role..."));
                    return;
                }
                guildDb.DefaultRoleId = role.Id;
                guildDb.HasDefaultRole = true;
                await soraContext.SaveChangesAsync();
            }
            await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0], $"Successfully{(wasCreated ? " created and" : "")} added {roleName} as default join role!"));
        }

        public async Task AddSarToList(SocketCommandContext context, string roleName, bool canExpire = false, int cost = 0, TimeSpan expireAt = new TimeSpan())
        {
            //check perms
            if (await Utility.HasAdminOrSoraAdmin(context) == false)
                return;
            var sora = context.Guild.CurrentUser;
            //Try to find role
            IRole role = context.Guild.Roles.FirstOrDefault(x =>
                x.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase));
            bool wasCreated = false;
            //Role wasn't found
            if (role == null)
            {
                //Check if sora can create a role
                if (!sora.GuildPermissions.Has(GuildPermission.ManageRoles))
                {
                    await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                        Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                        "The specified role was not found and Sora doesn't have ManageRoles permissions to create it!"));
                    return;
                }
                //he has the perms so he can create the role
                role = await context.Guild.CreateRoleAsync(roleName, GuildPermissions.None);
                wasCreated = true;
            }
            else
            {
                //check if the role found is ABOVE sora if so.. quit. (on life)
                var soraHighestRole = sora.Roles.OrderByDescending(x => x.Position).FirstOrDefault();
                //Sora is below in the hirarchy
                if (soraHighestRole.Position < role.Position)
                {
                    await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                        Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "I cannot assign roles that are above me in the role hirachy!")
                        .WithDescription("If this is not the case, open the server settings and move a couple roles around since discord doesn't refresh the position unless they are moved."));
                    return;
                }
            }
            //role was either found or created by sora.. 
            using (SoraContext soraContext = new SoraContext())
            {
                //check if it already exists
                var guildDb = Utility.GetOrCreateGuild(context.Guild.Id, soraContext);
                if (guildDb.SelfAssignableRoles.Count > 0 && guildDb.SelfAssignableRoles.Any(x => x.RoleId == role.Id))
                {
                    await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                        Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "This role is already self assignable!"));
                    return;
                }
                //Add it to the list!
                guildDb.SelfAssignableRoles.Add(new Role()
                {
                    CanExpire = canExpire,
                    Cost = cost,
                    Duration = expireAt,
                    GuildForeignId = context.Guild.Id,
                    RoleId = role.Id
                });
                //save DB
                await soraContext.SaveChangesAsync();
            }
            await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0], $"Successfully{(wasCreated ? " created and" : "")} added {roleName} to the list of self-assignable roles!"));
        }

        public async Task IAmNotSar(SocketCommandContext context, string roleName)
        {
            var sora = context.Guild.CurrentUser;
            //Check if sora can create a role
            if (!sora.GuildPermissions.Has(GuildPermission.ManageRoles))
            {
                await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                    Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                    "Sora doesn't have ManageRoles Permission. Please notify an admin!"));
                return;
            }

            //check if the user has the role
            var user = (SocketGuildUser)context.User;
            var role = user.Roles.FirstOrDefault(x => x.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase));
            if (role == null)
            {
                await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                    Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                    "You don't carry this role!"));
                return;
            }

            using (SoraContext soraContext = new SoraContext())
            {
                //check if the role is self assignable
                var guildDb = Utility.GetOrCreateGuild(context.Guild.Id, soraContext);
                if (guildDb.SelfAssignableRoles.All(x => x.RoleId != role.Id))
                {
                    await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                        Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                        "This role is not self-assignable!"));
                    return;
                }
                //user carries role and IS self assignable
                await user.RemoveRoleAsync(role);
            }
            await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0],
                $"Successfully removed {role.Name} from your roles!"));
        }

        public async Task ListSars(SocketCommandContext context)
        {
            using (SoraContext soraContext = new SoraContext())
            {
                var guildDb = Utility.GetOrCreateGuild(context.Guild.Id, soraContext);

                int roleCount = guildDb.SelfAssignableRoles.Count;
                if (roleCount == 0)
                {
                    await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                        Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                        "Guild has no self-assignable roles!"));
                    return;
                }
                if (roleCount < 24)
                {
                    var eb = new EmbedBuilder()
                    {
                        Color = Utility.PurpleEmbed,
                        Title = $"Self-Assignable roles in {context.Guild.Name}",
                        ThumbnailUrl = context.Guild.IconUrl ?? Utility.StandardDiscordAvatar,
                        Footer = Utility.RequestedBy(context.User)
                    };

                    List<Role> roleList = new List<Role>(guildDb.SelfAssignableRoles);
                    foreach (var role in roleList)
                    {
                        //check if role still exists otherwise remove it
                        var roleInfo = context.Guild.GetRole(role.RoleId);
                        if (roleInfo == null)
                        {
                            guildDb.SelfAssignableRoles.Remove(role);
                            continue;
                        }
                        eb.AddField(x =>
                        {
                            x.IsInline = true;
                            x.Name = roleInfo.Name;
                            x.Value =
                                $"Cost: {role.Cost}{(role.CanExpire ? $"\nDuration: {role.Duration.ToString(@"ww\.dd\.hh\")}" : "")}";
                        });
                    }
                    await context.Channel.SendMessageAsync("", embed: eb);
                }
                else
                {
                    //TODO PAGINATE
                    List<Role> roleList = new List<Role>(guildDb.SelfAssignableRoles);
                    List<string> sars = new List<string>();
                    int pageAmount = (int)Math.Ceiling(roleCount / 7.0);
                    int addToJ = 0;
                    int amountLeft = roleCount;
                    for (int i = 0; i < pageAmount; i++)
                    {
                        string addToList = "";
                        for (int j = 0; j < (amountLeft > 7 ? 7 : amountLeft); j++)
                        {
                            var role = roleList[j + addToJ];
                            var roleInfo = context.Guild.GetRole(role.RoleId);
                            if (roleInfo == null)
                            {
                                guildDb.SelfAssignableRoles.Remove(role);
                                continue;
                            }
                            addToList += $"**{roleInfo.Name}**\nCost: {role.Cost}{(role.CanExpire ? $" \t \tDuration: {role.Duration.ToString(@"ww\.dd\.hh\")}" : "")}";
                        }
                        sars.Add(addToList);
                        amountLeft -= 7;
                        addToJ += 7;
                    }
                    var pmsg = new PaginatedMessage()
                    {
                        Author = new EmbedAuthorBuilder()
                        {
                            IconUrl = context.User.GetAvatarUrl() ?? Utility.StandardDiscordAvatar,
                            Name = context.User.Username
                        },
                        Color = Utility.PurpleEmbed,
                        Title = $"Self-Assignable roles in {context.Guild.Name}",
                        Options = new PaginatedAppearanceOptions()
                        {
                            DisplayInformationIcon = false,
                            Timeout = TimeSpan.FromSeconds(60),
                            InfoTimeout = TimeSpan.FromSeconds(60)
                        },
                        Content = "Only the invoker may switch pages, ⏹ to stop the pagination",
                        Pages = sars
                    };

                    Criteria<SocketReaction> criteria = new Criteria<SocketReaction>();
                    criteria.AddCriterion(new EnsureReactionFromSourceUserCriterionMod());

                    await _interactive.SendPaginatedMessageAsync(context, pmsg, criteria);
                }
                await soraContext.SaveChangesAsync();
            }
        }

        public async Task IAmSar(SocketCommandContext context, string roleName)
        {
            var sora = context.Guild.CurrentUser;
            //Check if sora can create a role
            if (!sora.GuildPermissions.Has(GuildPermission.ManageRoles))
            {
                await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                    Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                    "Sora doesn't have ManageRoles Permission. Please notify an admin!"));
                return;
            }
            //check if role exists
            var role = context.Guild.Roles.FirstOrDefault(x =>
                x.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase));
            if (role == null)
            {
                await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                    Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                    "This role does not exist!"));
                return;
            }
            //Check if he already has the role
            var user = (SocketGuildUser)context.User;

            if (user.Roles.Any(x => x.Id == role.Id))
            {
                await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                    Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                    "You already have this role!"));
                return;
            }

            using (SoraContext soraContext = new SoraContext())
            {
                //check if the role is self assignable
                var guildDb = Utility.GetOrCreateGuild(context.Guild.Id, soraContext);
                if (guildDb.SelfAssignableRoles.All(x => x.RoleId != role.Id))
                {
                    await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                        Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                        "This role is not self-assignable!"));
                    return;
                }
                //role exists and IS self assignable
                await user.AddRoleAsync(role);
            }
            await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0],
                $"Successfully added {role.Name} to your roles!"));

        }
    }
}