using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.WebSocket;
using Humanizer;
using Humanizer.Localisation;
using Microsoft.Extensions.DependencyInjection;
using SoraBot_v2.Data;
using SoraBot_v2.Data.Entities.SubEntities;
using SoraBot_v2.Extensions;
using Timer = System.Threading.Timer;

namespace SoraBot_v2.Services
{
    public class SelfAssignableRolesService
    {
        private InteractiveService _interactive;
        private DiscordSocketClient _client;
        private Timer _timer;

        public SelfAssignableRolesService(InteractiveService service, DiscordSocketClient client)
        {
            _interactive = service;
            _client = client;
        }

        public void Initialize()
        {
            SetTimer();
        }

        private const int INITIAL_DELAY = 40;

        private void SetTimer()
        {
            _timer = new Timer(CheckExpiringRoles, null, TimeSpan.FromSeconds(INITIAL_DELAY),
                TimeSpan.FromSeconds(INITIAL_DELAY));
        }

        private async void CheckExpiringRoles(Object stateInfo)
        {
            try
            {
                using (var soraContext = new SoraContext())
                {
                    var roles = new List<ExpiringRole>();
                    roles = soraContext.ExpiringRoles.ToList();
                    foreach (var role in roles)
                    {
                        // get guild
                        var guild = _client.GetGuild(role.GuildForeignId);
                        if(guild == null)
                            continue;
                        // get user
                        var user = guild.GetUser(role.UserForeignId);
                        // if user isnt in guild anymore remove entry
                        if (user == null)
                        {
                            soraContext.ExpiringRoles.Remove(role);
                            continue;
                        }
                        // get role
                        var r = guild.GetRole(role.RoleForeignId);
                        // remove if role doesnt exist anymore
                        if (r == null)
                        {
                            soraContext.ExpiringRoles.Remove(role);
                            continue; 
                        }
                        // check if user still has role
                        if (user.Roles.All(x => x.Id != role.RoleForeignId))
                        {
                            // user doesnt have role anymore. remove
                            soraContext.ExpiringRoles.Remove(role);
                            continue; 
                        }
                        if (role.ExpiresAt.CompareTo(DateTime.UtcNow) <= 0)
                        {
                            // otherwise remove role from him and entry
                            // ratelimit is super strict here so what we do is try it, 
                            // if it throws an exception we wait 2 seconds and try again. Hopefully that works.
                            // otherwise we run again and retry.
                            try
                            {
                                await user.RemoveRoleAsync(r);
                                soraContext.ExpiringRoles.Remove(role);
                            }
                            catch (Exception e)
                            {
                                await Task.Delay(3000); // Role ratelimit is quite severe. so after removing one role we'll just wait since this is no pushing task.
                                await user.RemoveRoleAsync(r);
                                soraContext.ExpiringRoles.Remove(role);
                            }
                        }
                    }
                    await soraContext.SaveChangesAsync();
                }
                ChangeToClosestInterval();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void ChangeToClosestInterval()
        {
            using (var _soraContext = new SoraContext())
            {
                if (_soraContext.ExpiringRoles.ToList().Count == 0)
                {
                    _timer.Change(Timeout.Infinite, Timeout.Infinite);
                    return;
                }

                var sortedRoles = _soraContext.ExpiringRoles.ToList().OrderBy(x => x.ExpiresAt).ToList();
                var time = sortedRoles[0].ExpiresAt.Subtract(DateTime.UtcNow).TotalSeconds;
                if (time < 0)
                {
                    time = 0;
                }
                if (time > 86400)
                {
                    //just set timer to 1 day
                    _timer.Change(TimeSpan.FromDays(1), TimeSpan.FromDays(1));
                }
                else
                {
                    _timer.Change(TimeSpan.FromSeconds(time), TimeSpan.FromSeconds(time));
                }
            }
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
                if (soraHighestRole?.Position < role.Position)
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
                var roleDb = guildDb.SelfAssignableRoles.FirstOrDefault(x => x.RoleId == role.Id);
                if (roleDb == null)
                {
                    await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                        Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                        "This role is not self-assignable!"));
                    return;
                }
                //user carries role and IS self assignable
                // check if it had a duration
                if (roleDb.CanExpire)
                {
                    // remove entry in that list if it exists
                    var expireDb = soraContext.ExpiringRoles.FirstOrDefault(x => x.RoleForeignId == role.Id);
                    if (expireDb != null)
                    {
                        soraContext.ExpiringRoles.Remove(expireDb);
                        await soraContext.SaveChangesAsync();
                        ChangeToClosestInterval();
                    }
                }
                
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
                                $"Cost: {role.Cost}{(role.CanExpire ? $"\nDuration: {role.Duration.Humanize(2, maxUnit: TimeUnit.Day, minUnit: TimeUnit.Second, countEmptyUnits:true)}" : "")}";
                        });
                        //eb.Description += $"• {roleInfo.Name}\n";
                    }
                    await context.Channel.SendMessageAsync("", embed: eb);
                }
                else
                {
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
                            addToList += $"**{roleInfo.Name}**\nCost: {role.Cost}{(role.CanExpire ? $" \t \tDuration: {role.Duration.Humanize(2, maxUnit: TimeUnit.Day, minUnit: TimeUnit.Second, countEmptyUnits:true)} days" : "")}";
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
                var roleDb = guildDb.SelfAssignableRoles.FirstOrDefault(x => x.RoleId == role.Id);
                if (roleDb == null)
                {
                    await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                        Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                        "This role is not self-assignable!"));
                    return;
                }
                // role exists and IS self assignable
                // check if it costs. 
                if (roleDb.Cost > 0)
                {
                    // check if user has enough money.
                    var userDb = Utility.GetOrCreateUser(user.Id, soraContext);
                    if (userDb.Money < roleDb.Cost)
                    {
                        await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                            Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            "You don't have enough Sora Coins for this role."));
                        return;
                    }
                    // He has enough SC to buy
                    // Get owner db
                    var ownerDb = Utility.GetOrCreateUser(context.Guild.OwnerId, soraContext);
                    userDb.Money -= roleDb.Cost;
                    // only send 50% of it to the owner. the rest is tax to remove money from the economy.
                    ownerDb.Money += (int)Math.Floor(roleDb.Cost / 2.0);
                    // check if duration
                    if (roleDb.CanExpire)
                    {
                        // add role to list of expiring roles.
                        soraContext.ExpiringRoles.Add(new ExpiringRole()
                        {
                            RoleForeignId = role.Id,
                            ExpiresAt = DateTime.UtcNow.Add(roleDb.Duration),
                            GuildForeignId = context.Guild.Id,
                            UserForeignId = user.Id
                        });
                        Console.WriteLine($"EXPIRES AT: {DateTime.UtcNow.Add(roleDb.Duration)}");
                    }
                    await soraContext.SaveChangesAsync();
                    ChangeToClosestInterval();
                }                
                await user.AddRoleAsync(role);
            }
            await context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(
                Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0],
                $"Successfully added {role.Name} to your roles!"));
        }
    }
}