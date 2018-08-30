using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using SoraBot_v2.Data;
using SoraBot_v2.Data.Entities.SubEntities;

namespace SoraBot_v2.Services
{
    public class GuildLevelRoleService
    {

        public static string DEFAULT_MSG = "Well done {user}, you reached level {level}!";
        
        public async Task OnUserExpGain(int epGain, SocketCommandContext context)
        {
            using (var soraContext = new SoraContext())
            {
                var user = context.User as SocketGuildUser;
                if (user == null)
                {
                    Console.WriteLine("ROLE USER NULL");
                    return;
                }
                var guildDb = Utility.GetOrCreateGuild(context.Guild.Id, soraContext);
                var banned = guildDb.LevelRoles.Where(x => x.Banned).ToList();
                //check if user has any of the banned roles
                if (banned.Count != 0)
                {
                    if (banned.Any(role => user.Roles.Any(x=> x.Id == role.RoleId)))
                    {
                        return;
                    }
                    //No banned roles.
                }
                //user can gain EXP
                var guildUser = Utility.GetOrCreateGuildUser(user.Id, context.Guild.Id, soraContext);
                int previousLevel = ExpService.CalculateLevel(guildUser.Exp);
                guildUser.Exp += epGain;
                await soraContext.SaveChangesAsync();
                //gained ep, do the rest
                int currentLevel = ExpService.CalculateLevel(guildUser.Exp);
                
                //if no lvl up occured, there is no need to do anything.
                if(previousLevel == currentLevel)
                    return;
                
                //there are no level roles
                if(guildDb.LevelRoles.Count == 0)
                    return;

                //send lvl up message
                if (guildDb.EnabledLvlUpMessage)
                {
                    var msg = (string.IsNullOrWhiteSpace(guildDb.LevelUpMessage)
                        ? DEFAULT_MSG
                        : guildDb.LevelUpMessage);
                    msg = EditMsg(msg, user, currentLevel);
                    try
                    {
                        if (guildDb.SendLvlDm)
                        {
                            var eb = new EmbedBuilder()
                            {
                                Author = new EmbedAuthorBuilder()
                                {
                                    IconUrl = context.Guild.IconUrl ?? Utility.StandardDiscordAvatar,
                                    Name = context.Guild.Name
                                },
                                Description = msg,
                                Color = Utility.BlueInfoEmbed
                            };
                            await (await user.GetOrCreateDMChannelAsync()).SendMessageAsync("", embed: eb.Build());
                        }
                        else
                        {
                            await context.Channel.SendMessageAsync(msg);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
                //Give roles
                var roles = guildDb.LevelRoles.Where(x => x.RequiredLevel == currentLevel && !x.Banned).ToList();
                if(roles.Count == 0)
                    return;
                
                //Check if sora has manage roles permission!
                var sora = context.Guild.CurrentUser;
                if (!sora.GuildPermissions.Has(GuildPermission.ManageRoles))
                {
                    //try to send a DM to the owner
                    try
                    {
                        await (await context.Guild.Owner.GetOrCreateDMChannelAsync()).SendMessageAsync("",
                            embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], 
                                    $"Sora is missing crucial permissions!")
                                .WithDescription($"You've set up Roles as level rewards but Sora is missing the Manage Roles permission in {context.Guild.Name}! " +
                                                 $"{Utility.GiveUsernameDiscrimComb(context.User)} earned a new role but couldn't receive it due to the missing permissions! " +
                                                 $"This message will be sent to you every time a user levels up and would receive a new role. Either remove the role rewards or grant Sora the missing permissions!").Build());
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                    await soraContext.SaveChangesAsync();
                    return;
                }
                
                foreach (var role in roles)
                {
                    //check if he already has it
                    if(user.Roles.Any(x=> x.Id == role.RoleId))
                        continue;
                    //if not add
                    var addRole = context.Guild.GetRole(role.RoleId);
                    //check if role still exists
                    if (addRole == null)
                    {
                        //remove role from list of roles and continue
                        guildDb.LevelRoles.Remove(role);
                        continue;
                    }
                    await user.AddRoleAsync(addRole);
                }
                await soraContext.SaveChangesAsync();
            }  
        }

        private string EditMsg(string msg, SocketGuildUser user, int lvl)
        {
            msg = msg.Replace("{user}", user.Mention);
            msg = msg.Replace("{level}", lvl.ToString());
            return msg;
        }
    }
}