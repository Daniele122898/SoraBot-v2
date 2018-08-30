using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SoraBot_v2.Data;
using SoraBot_v2.Data.Entities;
using SoraBot_v2.Data.Entities.SubEntities;

namespace SoraBot_v2.Services
{
    public class ClanService
    {
        
        private InteractiveService _interactive;
        private DiscordRestClient _restClient;

        public ClanService(InteractiveService interactive, DiscordRestClient restClient)
        {
            _interactive = interactive;
            _restClient = restClient;
        }

        private const int MINIMUM_CREATE_LEVEL = 5;
        private const int MAX_MEMBERCOUNT = 20;
        
        private const int CLAN_CREATION_COST = 2000;
        private const int CLAN_RENAME_COST = 500;
        private const int CLAN_LVLUP_COST = 10000;
        
        private struct ClanListing
        {
            public Clan Clan { get; set; }
            public float TotalExp { get; set; }
        }
        
        private int GetMaxUsers(Clan clan)
        {
            return MAX_MEMBERCOUNT + (clan.Level*10);
        }

        public async Task LevelUpClan(SocketCommandContext context)
        {
            using (var soraContext = new SoraContext())
            {
                // check if user is in a clan
                var userDb = Utility.OnlyGetUser(context.User.Id, soraContext);
                if (userDb == null)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "You are not in a clan!"));
                    return;
                }
                if (string.IsNullOrWhiteSpace(userDb.ClanName))
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "You are not in a clan!"));
                    return; 
                }
                // check if clan still exists
                var clan = Utility.GetClan(userDb.ClanName, soraContext);
                if (clan == null)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Your clan doesn't exist anymore..."));
                    return;
                }
                // otherwise check if he's owner or staff
                if (clan.OwnerId != context.User.Id && !userDb.ClanStaff)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "You are not the owner of this clan nor a staff member!"));
                    return;
                }
                // check available money
                if (userDb.Money < CLAN_LVLUP_COST)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"You don't have enough SC to upgrade! It costs {CLAN_LVLUP_COST} SC!"));
                    return;
                }
                // upgrade clan
                clan.Level += 1;
                // remove money
                userDb.Money -= CLAN_LVLUP_COST;
                // save changes
                await soraContext.SaveChangesAsync();
                
                await context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(
                        Utility.GreenSuccessEmbed, 
                        Utility.SuccessLevelEmoji[0], 
                        $"Successfully upgraded the clan! You can now have {GetMaxUsers(clan)} members!"));
            }
        }

        public async Task RenameClan(SocketCommandContext context, string clanName)
        {
            using (var soraContext = new SoraContext())
            {
                // check if user is in a clan
                var userDb = Utility.OnlyGetUser(context.User.Id, soraContext);
                if (userDb == null)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "You are not in a clan!"));
                    return;
                }
                if (string.IsNullOrWhiteSpace(userDb.ClanName))
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "You are not in a clan!"));
                    return; 
                }
                // check if clan still exists
                var clan = Utility.GetClan(userDb.ClanName, soraContext);
                if (clan == null)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Your clan doesn't exist anymore..."));
                    return;
                }
                // otherwise check if he's owner
                if (clan.OwnerId != context.User.Id)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "You are not the owner of this clan!"));
                    return;
                }
                // check available money
                if (userDb.Money < CLAN_RENAME_COST)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"You don't have enough SC to rename! It costs {CLAN_RENAME_COST} SC!"));
                    return;
                }
                // he's owner and has the cash. rename 
                clan.Name = clanName;
                // update all members
                foreach (var member in clan.Members)
                {
                    member.ClanName = clanName;
                }
                // update this user
                userDb.ClanName = clanName;
                // remove some of his cash KEK
                userDb.Money -= CLAN_RENAME_COST;
                
                await soraContext.SaveChangesAsync();
                
                await context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(
                        Utility.GreenSuccessEmbed, 
                        Utility.SuccessLevelEmoji[0], 
                        $"Successfully renamed the clan to `{clanName}`"));
            }            
        }
        
        
        public async Task ShowClanList(SocketCommandContext context)
        {
            using (var soraContext = new SoraContext())
            {
                // if no clans are made
                if (!soraContext.Clans.Any())
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "No Clans exist yet. Create one!"));
                    return;
                }
                
                List<ClanListing> list = new List<ClanListing>();
                // create list
                foreach (var clan in soraContext.Clans)
                {
                    var entry = new ClanListing {Clan = Utility.GetClan(clan, soraContext)};
                    foreach (var member in clan.Members)
                    {
                        entry.TotalExp += member.Exp;
                    }
                    list.Add(entry);
                }
                
                // sort list and pick top 10 :>
                var sorted = list.OrderByDescending(x => x.TotalExp).Take(10).ToList();
                
                // prepare list to display :>
                // lets actually only show the top 10 clans :)
                
                var eb = new EmbedBuilder()
                {
                    Title = "Top 10 Clans",
                    ThumbnailUrl = sorted[0].Clan.AvatarUrl ?? context.Client.CurrentUser.GetAvatarUrl(),
                    Description = "Top 10 Clans sorted by total EXP of all members",
                    Footer = Utility.RequestedBy(context.User),
                    Color = Utility.PurpleEmbed
                };

                short count = 1;
                foreach (var clan in sorted)
                {
                    // only show 20 for lvl up shit.
                    if (count > 20)
                        break;
                    
                    eb.AddField((x) =>
                    {
                        x.IsInline = false;
                        x.Name = $"{(count)}. {clan.Clan.Name}";
                        x.Value = $"{clan.TotalExp} EXP";
                    });
                    count++;
                }
                await context.Channel.SendMessageAsync("", embed: eb);
            }
        }

        public async Task RemoveClan(SocketCommandContext context)
        {
            using (var soraContext = new SoraContext())
            {
                var userDb = Utility.OnlyGetUser(context.User.Id, soraContext);
                if (string.IsNullOrWhiteSpace(userDb?.ClanName))
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            $"You are not part of a clan!"));
                    return;
                }
                var clan = Utility.GetClan(userDb.ClanName, soraContext);
                if (clan == null)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            $"Your clan doesn't exist anymore. You have now been removed."));
                    userDb.ClanName = null;
                    userDb.ClanStaff = false;
                    await soraContext.SaveChangesAsync();
                    return;
                }
                if (context.User.Id != clan.OwnerId)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            $"You are not the owner of this guild!"));
                    return;
                }
                var eb = new EmbedBuilder()
                {
                    Color = Utility.YellowWarningEmbed,
                    Title = Utility.SuccessLevelEmoji[1]+ " Are you sure you want to delete " + clan.Name+"?",
                    Description = "This cannot be undone! All members will be kicked and the entire clan will be erased.\n If you are certain answer with `y` or `yes`. Otherwise just enter anything."
                };
                await context.Channel.SendMessageAsync("", embed: eb);

                var response = await _interactive.NextMessageAsync(context, true, true, TimeSpan.FromSeconds(45));
                if (response == null)
                {
                    await context.Channel.SendMessageAsync("", embed:
                        Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Didn't answer in time ;_;"));
                    return;
                }

                if (response.Content.Equals("y", StringComparison.OrdinalIgnoreCase) ||
                    response.Content.Equals("yes", StringComparison.OrdinalIgnoreCase))
                {
                    //remove all users
                    foreach (var member in clan.Members)
                    {
                        member.ClanName = null;
                        member.ClanStaff = false;
                    }
                    //remove all invites
                    var invites = soraContext.ClanInvites.Where(x => x.ClanName.Equals(clan.Name)).ToList();
                    soraContext.ClanInvites.RemoveRange(invites);
                    //remove clan
                    soraContext.Clans.Remove(clan);
                    await soraContext.SaveChangesAsync();
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0], $"Successfully removed {clan.Name}!"));
                }
                else
                {
                    await context.Channel.SendMessageAsync("", embed:
                        Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Didn't answer with y or yes!"));
                }
               
            }
        }

        public async Task MakeOwner(SocketCommandContext context, SocketUser user)
        {
            using (var soraContext = new SoraContext())
            {
                var userDb = Utility.OnlyGetUser(context.User.Id, soraContext);
                if (string.IsNullOrWhiteSpace(userDb?.ClanName))
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            $"You are not part of a clan!"));
                    return;
                }
                var clan = Utility.GetClan(userDb.ClanName, soraContext);
                if (clan == null)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            $"Your clan doesn't exist anymore..."));
                    return;
                }
                if (context.User.Id != clan.OwnerId)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            $"You are not the owner of this guild!"));
                    return;
                }
                //Get other user
                var kuserDb = Utility.OnlyGetUser(user.Id, soraContext);
                if (string.IsNullOrWhiteSpace(kuserDb?.ClanName))
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            $"{Utility.GiveUsernameDiscrimComb(user)} is not part of a clan!"));
                    return;
                }
                //check if same clan
                if (!kuserDb.ClanName.Equals(clan.Name, StringComparison.OrdinalIgnoreCase))
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            $"{Utility.GiveUsernameDiscrimComb(user)} is not part of your clan!"));
                    return;
                }
                
                //make owner
                kuserDb.ClanStaff = true;
                clan.OwnerId = user.Id;
                userDb.ClanStaff = true;
                await soraContext.SaveChangesAsync();
                await context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0], $"Successfully set {Utility.GiveUsernameDiscrimComb(user)} as new owner!"));
                //Send DM to user who got promoted.
                try
                {
                    await (await user.GetOrCreateDMChannelAsync()).SendMessageAsync("", embed: Utility.ResultFeedback(Utility.BlueInfoEmbed, Utility.SuccessLevelEmoji[3], $"You have been promoted to clan owner!"));
                }
                catch (Exception e)
                {
                    // ignored
                }
            }
        }

        public async Task RemoveStaff(SocketCommandContext context, SocketUser user)
        {
            using (var soraContext = new SoraContext())
            {
                var userDb = Utility.OnlyGetUser(context.User.Id, soraContext);
                if (string.IsNullOrWhiteSpace(userDb?.ClanName))
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            $"You are not part of a clan!"));
                    return;
                }
                if (!userDb.ClanStaff)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            $"You have to be a staff member to remove other staff members!"));
                    return;
                }
                var clan = Utility.GetClan(userDb.ClanName, soraContext);
                if (clan == null)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            $"Your clan doesn't exist anymore..."));
                    return;
                }
                //Get other user
                var kuserDb = Utility.OnlyGetUser(user.Id, soraContext);
                if (string.IsNullOrWhiteSpace(kuserDb?.ClanName))
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            $"{Utility.GiveUsernameDiscrimComb(user)} is not part of a clan!"));
                    return;
                }
                //check if same clan
                if (!kuserDb.ClanName.Equals(clan.Name, StringComparison.OrdinalIgnoreCase))
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            $"{Utility.GiveUsernameDiscrimComb(user)} is not part of your clan!"));
                    return;
                }
                //check if he's owner
                if (user.Id == clan.OwnerId)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            $"You can't remove staff from the owner!"));
                    return;
                }
                //check if already staff
                if (!kuserDb.ClanStaff)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            $"User is not a staff member!"));
                    return;  
                }
                //remove staff
                kuserDb.ClanStaff = false;
                await soraContext.SaveChangesAsync();
                await context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0], $"Successfully removed {Utility.GiveUsernameDiscrimComb(user)} from the staff team!"));
                //Send DM to user who got promoted.
                try
                {
                    await (await user.GetOrCreateDMChannelAsync()).SendMessageAsync("", embed: Utility.ResultFeedback(Utility.BlueInfoEmbed, Utility.SuccessLevelEmoji[3], $"You have been removed from the staff team!"));
                }
                catch (Exception e)
                {
                    // ignored
                }
            }
        }

        public async Task MakeStaff(SocketCommandContext context, SocketUser user)
        {
            using (var soraContext = new SoraContext())
            {
                var userDb = Utility.OnlyGetUser(context.User.Id, soraContext);
                if (string.IsNullOrWhiteSpace(userDb?.ClanName))
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            $"You are not part of a clan!"));
                    return;
                }
                if (!userDb.ClanStaff)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            $"You have to be a staff member to make other members staff!"));
                    return;
                }
                var clan = Utility.GetClan(userDb.ClanName, soraContext);
                if (clan == null)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            $"Your clan doesn't exist anymore..."));
                    return;
                }
                //Get other user
                var kuserDb = Utility.OnlyGetUser(user.Id, soraContext);
                if (string.IsNullOrWhiteSpace(kuserDb?.ClanName))
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            $"{Utility.GiveUsernameDiscrimComb(user)} is not part of a clan!"));
                    return;
                }
                //check if same clan
                if (!kuserDb.ClanName.Equals(clan.Name, StringComparison.OrdinalIgnoreCase))
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            $"{Utility.GiveUsernameDiscrimComb(user)} is not part of your clan!"));
                    return;
                }
                //check if he's owner
                if (user.Id == clan.OwnerId)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            $"Owner is always staff..."));
                    return;
                }
                //check if already staff
                if (kuserDb.ClanStaff)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            $"User is a staff member already!"));
                    return;  
                }
                //make user staff
                kuserDb.ClanStaff = true;
                await soraContext.SaveChangesAsync();
                await context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0], $"Successfully added {Utility.GiveUsernameDiscrimComb(user)} to the staff team!"));
                //Send DM to user who got promoted.
                try
                {
                    await (await user.GetOrCreateDMChannelAsync()).SendMessageAsync("", embed: Utility.ResultFeedback(Utility.BlueInfoEmbed, Utility.SuccessLevelEmoji[3], $"You have been promoted to staff!"));
                }
                catch (Exception e)
                {
                    // ignored
                }
            }
        }

        public async Task KickUser(SocketCommandContext context, SocketUser user)
        {
            using (var soraContext = new SoraContext())
            {
                var userDb = Utility.OnlyGetUser(context.User.Id, soraContext);
                if (string.IsNullOrWhiteSpace(userDb?.ClanName))
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"You are not part of a clan!"));
                    return; 
                }
                if (!userDb.ClanStaff)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"You have to be a staff member to kick other users!"));
                    return; 
                }
                var clan = Utility.GetClan(userDb.ClanName, soraContext);
                if (clan == null)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"Your clan doesn't exist anymore..."));
                    return; 
                }
                //Get other user
                var kuserDb = Utility.OnlyGetUser(user.Id, soraContext);
                if (string.IsNullOrWhiteSpace(kuserDb?.ClanName))
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"{Utility.GiveUsernameDiscrimComb(user)} is not part of a clan!"));
                    return; 
                }
                //check if same clan
                if (!kuserDb.ClanName.Equals(clan.Name, StringComparison.OrdinalIgnoreCase))
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"{Utility.GiveUsernameDiscrimComb(user)} is not part of your clan!"));
                    return; 
                }
                //check if he's owner
                if (user.Id == clan.OwnerId)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"You cannot kick the owner of the clan!"));
                    return; 
                }
                //kick user
                kuserDb.ClanName = null;
                kuserDb.ClanStaff = false;
                await soraContext.SaveChangesAsync();
                await context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0], $"Successfully kicked {Utility.GiveUsernameDiscrimComb(user)}!"));
                //Send DM to user who got kicked.
                try
                {
                        await (await user.GetOrCreateDMChannelAsync()).SendMessageAsync("", embed: Utility.ResultFeedback(Utility.BlueInfoEmbed, Utility.SuccessLevelEmoji[3], $"You have been kicked from {clan.Name}!"));
                }
                catch (Exception e)
                {
                    // ignored
                }
            }
        }

        public async Task LeaveClan(SocketCommandContext context)
        {
            using (var soraContext = new SoraContext())
            {
                var userDb = Utility.OnlyGetUser(context.User.Id, soraContext);
                if (string.IsNullOrWhiteSpace(userDb?.ClanName))
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"You are not part of a clan!"));
                    return; 
                }
                var clan = Utility.GetClan(userDb.ClanName, soraContext);
                if (clan == null)
                {
                    //CLan doesnt exist anymore so remove his aquintance
                    userDb.ClanName = null;
                    userDb.ClanStaff = false;
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0], $"Successfully left the clan."));
                    await soraContext.SaveChangesAsync();
                    return; 
                }
                if (clan.OwnerId == context.User.Id)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"You cannot leave a clan you own. Transfer ownership or remove it completely!"));
                    return; 
                }
                userDb.ClanName = null;
                userDb.ClanStaff = false;
                await context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0], $"Successfully left the clan."));
                await soraContext.SaveChangesAsync();
            }
        }

        public async Task EditClanDescription(SocketCommandContext context, string clanMessage)
        {
            using (var soraContext = new SoraContext())
            {
                var userDb = Utility.OnlyGetUser(context.User.Id, soraContext);
                if (string.IsNullOrWhiteSpace(userDb?.ClanName))
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"You are not part of a clan!"));
                    return; 
                }
                if (!userDb.ClanStaff)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"You have to be a staff member to edit the clan description!"));
                    return; 
                }
                var clan = Utility.GetClan(userDb.ClanName, soraContext);
                if (clan == null)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"Your clan doesn't exist anymore..."));
                    return; 
                }
                clan.Message = clanMessage;
                await soraContext.SaveChangesAsync();
                await context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0], $"Successfully changed clan description!"));
            }
        }

        public async Task EditClanInvite(SocketCommandContext context, string clanName, bool accepted)
        {
            using (var soraContext = new SoraContext())
            {
                var invite = soraContext.ClanInvites.FirstOrDefault(x =>
                    x.ClanName.Equals(clanName, StringComparison.OrdinalIgnoreCase) && x.UserId == context.User.Id);
                if (invite == null)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"You have no invite from this clan!"));
                    return; 
                }
                Clan clan = null;
                //DECLINE INVITE
                if (!accepted)
                {
                    soraContext.ClanInvites.Remove(invite);
                    await soraContext.SaveChangesAsync();
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0],
                            $"Successfully declined Clan Invite!"));
                }
                else
                {
                    //check if already in a clan
                    var userDb = Utility.GetOrCreateUser(context.User.Id, soraContext);
                    if (!string.IsNullOrWhiteSpace(userDb.ClanName))
                    {
                        await context.Channel.SendMessageAsync("",
                            embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                                $"You are already part of a clan! Please leave first before joining another one!"));
                        return;
                    }
                    //check if clan still exists.
                    clan = Utility.GetClan(clanName, soraContext);
                    if (clan == null)
                    {
                        soraContext.ClanInvites.Remove(invite);

                        await context.Channel.SendMessageAsync("",
                            embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                                $"Clan does not exist anymore! Invite was removed."));
                        await soraContext.SaveChangesAsync();
                        return;
                    }
                    //CHECK IF CLAN IS FULLL
                    if (clan.Members.Count >= GetMaxUsers(clan))
                    {
                        await context.Channel.SendMessageAsync("",
                            embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                                $"Clan is already full! Wait until someone leaves..."));
                        await soraContext.SaveChangesAsync();
                        return;
                    }
                    userDb.ClanName = clan.Name;
                    userDb.ClanStaff = false;
                    userDb.JoinedClan = DateTime.UtcNow;
                    soraContext.ClanInvites.Remove(invite);
                    await soraContext.SaveChangesAsync();
                    //send messages
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0],
                            "You successfully joined " + clan.Name));
                }
                //try to send message to Moderator
                try
                {
                    var mod = context.Client.GetUser(invite.StaffId);
                    if (mod != null)
                    {
                        string state = (accepted ? "accepted" : "declined");
                        var eb = new EmbedBuilder()
                        {
                            Color = Utility.BlueInfoEmbed,
                            Title = $"{Utility.GiveUsernameDiscrimComb(context.User)} {state} your clan invite!",
                            Description = $"Your clan invite to {clan.Name} was {state}!"
                        };
                        await (await mod.GetOrCreateDMChannelAsync()).SendMessageAsync("", embed: eb);
                    }
                }
                catch (Exception e)
                {
                    // ignored
                }
            }
        }

        public async Task RevokeInvite(SocketCommandContext context, SocketUser user)
        {
            using (var soraContext = new SoraContext())
            {
                var userDb = Utility.OnlyGetUser(context.User.Id, soraContext);
                if (string.IsNullOrWhiteSpace(userDb?.ClanName))
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            $"You are not part of a clan!"));
                    return;
                }
                if (!userDb.ClanStaff)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            $"You are not a Staff member!"));
                    return;
                }
                var clan = Utility.GetClan(userDb.ClanName, soraContext);
                if (clan == null)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            $"Your clan doesn't exist anymore..."));
                    return;
                }
                //check if invite exists!
                var invite = soraContext.ClanInvites.FirstOrDefault(x =>
                    x.ClanName.Equals(clan.Name, StringComparison.OrdinalIgnoreCase) && x.UserId == user.Id);
                if (invite == null)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            $"This person has no invite yet!"));
                    return;
                }
                //remove invite
                soraContext.ClanInvites.Remove(invite);
                await soraContext.SaveChangesAsync();
                await context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0],
                        $"The invite has been revoked!").WithDescription("No DM will be sent to the user in this case."));
                return;
            }
        }

        public async Task InviteUser(SocketCommandContext context, SocketUser user)
        {
            using (var soraContext = new SoraContext())
            {
                var userDb = Utility.OnlyGetUser(context.User.Id, soraContext);
                if (string.IsNullOrWhiteSpace(userDb?.ClanName))
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"You are not part of a clan!"));
                    return; 
                }
                if (!userDb.ClanStaff)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"You are not a Staff member!"));
                    return; 
                }
                var clan = Utility.GetClan(userDb.ClanName, soraContext);
                if (clan == null)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"Your clan doesn't exist anymore..."));
                    return; 
                }
                //check if invite already exists!
                if (soraContext.ClanInvites.Any(x =>
                    x.ClanName.Equals(clan.Name, StringComparison.OrdinalIgnoreCase) && x.UserId == user.Id))
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"You already invited this person!"));
                    return; 
                }
                if (clan.Members.Count >= GetMaxUsers(clan))
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"You already reached the max amount of members!"));
                    return; 
                }
                //create Invite
                var invite = new ClanInvite(){ClanName = clan.Name, StaffId = context.User.Id, UserId = user.Id};
                soraContext.ClanInvites.Add(invite);
                await soraContext.SaveChangesAsync();
                //send invite
                bool send = true;
                try
                {
                    var eb = new EmbedBuilder()
                    {
                        Author = new EmbedAuthorBuilder()
                        {
                            IconUrl = context.User.GetAvatarUrl() ?? Utility.StandardDiscordAvatar,
                            Name = Utility.GiveUsernameDiscrimComb(context.User)
                        },
                        Color = Utility.BlueInfoEmbed,
                        Title = $"Clan Invite from {clan.Name}!",
                        Description = $"You have been invited to join {clan.Name}! You can accept this request using the `$accept {clan.Name}` command or decline it using the " +
                                      $"`$decline {clan.Name}` command! You must use them in a guild with Sora and his respective prefix, DM doesnt work. Or alternatively use the Sora Dashboard!"
                    };
                    await (await user.GetOrCreateDMChannelAsync()).SendMessageAsync("", embed: eb);
                }
                catch (Exception e)
                {
                    send = false;
                }
                var eb2 = Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0],
                    "User successfully invited!");
                if (!send)
                    eb2.Description = "Failed to send Clan Invite via DM! Please contact the user directly!";
                await context.Channel.SendMessageAsync("", embed: eb2);
            }
        }

        public async Task RmClanAvatar(SocketCommandContext context)
        {
            using (var soraContext = new SoraContext())
            {
                var userDb = Utility.OnlyGetUser(context.User.Id, soraContext);
                if (string.IsNullOrWhiteSpace(userDb?.ClanName))
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            $"You are not part of a clan!"));
                    return;
                }
                if (!userDb.ClanStaff)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            $"You must be a staff member to change the clan avatar!"));
                    return;
                }
                //Get Clan
                var clan = Utility.GetClan(userDb.ClanName, soraContext);
                if (clan == null)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            $"Your clan doesn't exist anymore :/"));
                    return;
                }
                
                clan.HasImage = false;
                await soraContext.SaveChangesAsync();
                await context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0],
                        "Successfully removed clan avatar!"));
            }
        }

        public async Task SetClanAvatar(SocketCommandContext context, string url)
        {
            using (var soraContext = new SoraContext())
            {
                var userDb = Utility.OnlyGetUser(context.User.Id, soraContext);
                if (string.IsNullOrWhiteSpace(userDb?.ClanName))
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            $"You are not part of a clan!"));
                    return;
                }
                if (!userDb.ClanStaff)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            $"You must be a staff member to change the clan avatar!"));
                    return;
                }
                //Get Clan
                var clan = Utility.GetClan(userDb.ClanName, soraContext);
                if (clan == null)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            $"Your clan doesn't exist anymore :/"));
                    return;
                }
                
                clan.HasImage = true;
                clan.AvatarUrl = url;
                await soraContext.SaveChangesAsync();
                await context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0],
                        "Successfully set new clan avatar!"));
            }
        }

        public async Task ShowClanInfo(SocketCommandContext context, string clanName)
        {
            using (var soraContext = new SoraContext())
            {
                if (string.IsNullOrWhiteSpace(clanName))
                {
                    var userDb = Utility.OnlyGetUser(context.User.Id, soraContext);
                    if (string.IsNullOrWhiteSpace(userDb?.ClanName))
                    {
                        await context.Channel.SendMessageAsync("",
                            embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"You are not in a clan! Enter a clan name!"));
                        return;
                    }
                    clanName = userDb.ClanName;
                }
                var clan = Utility.GetClan(clanName, soraContext);
                if (clan == null)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"Clan doesn't exist!"));
                    return;
                }
                
                // get total EXP
                float totalExp = 0;
                clan.Members.ForEach(x=> totalExp += x.Exp);
                
                string desc = (string.IsNullOrWhiteSpace(clan.Message) ? "Clan has no description!" : clan.Message) + $"\n**Total Exp:** {totalExp}";
                var eb = new EmbedBuilder()
                {
                    Title = $"{Utility.SuccessLevelEmoji[3]} {clan.Name} info",
                    Color = Utility.BlueInfoEmbed,
                    Description = desc,
                    Footer = new EmbedFooterBuilder()
                    {
                        Text = $"Requested by {Utility.GiveUsernameDiscrimComb(context.User)} | Created: {clan.Created} UTC",
                        IconUrl = context.User.GetAvatarUrl() ?? Utility.StandardDiscordAvatar
                    },
                    ThumbnailUrl = Utility.StandardDiscordAvatar
                };
                if (clan.HasImage)
                {
                    eb.WithThumbnailUrl(clan.AvatarUrl);
                }
                var members = clan.Members.OrderByDescending(x => x.Exp).ThenByDescending(x => x.ClanStaff).ToList();
                for (int i = 0; i < members.Count; i++)
                {
                    IUser user = context.Client.GetUser(members[i].UserId);
                    if (user == null)
                    {
                        user = await _restClient.GetUserAsync(members[i].UserId);
                    }
                    var userName = (user == null ? members[i].UserId.ToString() : Utility.GiveUsernameDiscrimComb(user));
                    eb.AddField(x =>
                    {
                        x.IsInline = true;
                        x.Name = $"{i + 1}. {(members[i].ClanStaff ? "[S] " : "")}{userName}";
                        x.Value =
                            $"Lvl. {ExpService.CalculateLevel(members[i].Exp)} \tEXP: {members[i].Exp}\n*Joined: {(members[i].JoinedClan.ToString("dd/MM/yyyy"))}*";
                    });
                }
                await context.Channel.SendMessageAsync("", embed: eb);
            }
        }

        public async Task CreateClan(SocketCommandContext context, string clanName)
        {
            using (var soraContext = new SoraContext())
            {
                //Check User Criterias
                var userDb = Utility.OnlyGetUser(context.User.Id, soraContext);
                if (userDb == null)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"You need to be at least level {MINIMUM_CREATE_LEVEL} to create a clan!"));
                    return;
                }
                if (!string.IsNullOrWhiteSpace(userDb.ClanName))
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"You are already in a clan! Please leave that clan first!").WithDescription("If you are the owner, you can pass on the clan to someone else before leaving."));
                    return;
                }
                if (ExpService.CalculateLevel(userDb.Exp) < MINIMUM_CREATE_LEVEL)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"You need to be at least level {MINIMUM_CREATE_LEVEL} to create a clan!"));
                    return;
                }
                
                // check his cash
                if (userDb.Money < CLAN_CREATION_COST)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"Creating a clan costs {CLAN_CREATION_COST} Sora Coins! You don't have enough!"));
                    return;
                }
                
                //check if clan already exists
                if (soraContext.Clans.Any(x => x.Name.Equals(clanName, StringComparison.OrdinalIgnoreCase)))
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Clan Name is already taken!"));
                    return;
                }
                
                //now we can create the clan!
                var clan = new Clan(){HasImage = false, Members = new List<User>(), Message = "", Name = clanName, OwnerId = context.User.Id, Created = DateTime.UtcNow};
                userDb.ClanName = clanName;
                userDb.JoinedClan = DateTime.UtcNow;
                clan.Members.Add(userDb);
                userDb.ClanStaff = true;
                userDb.Money -= CLAN_CREATION_COST;
                soraContext.Clans.Add(clan);
                await soraContext.SaveChangesAsync();
                await context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0], $"Clan \"{clanName}\" has been created!"));
            }
        }
    }
}