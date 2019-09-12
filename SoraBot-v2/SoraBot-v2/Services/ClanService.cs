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
        
        private readonly InteractiveService _interactive;
        private DiscordRestClient _restClient;
        private readonly CoinService _coinService;

        public ClanService(InteractiveService interactive, DiscordRestClient restClient,
                            CoinService coinService)
        {
            _interactive = interactive;
            _restClient = restClient;
            _coinService = coinService;
        }

        private const int MINIMUM_CREATE_LEVEL = 5;
        private const int MAX_MEMBERCOUNT = 20;
        
        private const int CLAN_CREATION_COST = 2000;
        private const int CLAN_RENAME_COST = 500;
        private const int CLAN_LVLUP_COST = 7500;
        private const int CLAN_MAX_LEVEL = 3;
        
        private struct ClanListing
        {
            public Clan Clan { get; set; }
            public float TotalExp { get; set; }
        }
        
        private int GetMaxUsers(Clan clan)
        {
            return MAX_MEMBERCOUNT + (clan.Level*5);
        }

        public async Task LevelUpClan(SocketCommandContext context)
        {
            using (var soraContext = new SoraContext())
            {
                var lck = _coinService.GetOrCreateLock(context.User.Id);
                try
                {
                    if (!await lck.WaitAsync(CoinService.LOCK_TIMOUT_MSECONDS))
                    {
                        await _coinService.LockingErrorMessage(context.Channel);
                        return;
                    }
                    // check if user is in a clan
                    var userDb = Utility.OnlyGetUser(context.User.Id, soraContext);
                    if (userDb == null)
                    {
                        await context.Channel.SendMessageAsync("",
                            embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "You are not in a clan!").Build());
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(userDb.ClanName))
                    {
                        await context.Channel.SendMessageAsync("",
                            embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "You are not in a clan!").Build());
                        return; 
                    }
                    // check if clan still exists
                    var clan = Utility.GetClan(userDb.ClanName, soraContext);
                    if (clan == null)
                    {
                        await context.Channel.SendMessageAsync("",
                            embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Your clan doesn't exist anymore...").Build());
                        return;
                    }
                    // otherwise check if he's owner or staff
                    if (clan.OwnerId != context.User.Id && !userDb.ClanStaff)
                    {
                        await context.Channel.SendMessageAsync("",
                            embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "You are not the owner of this clan nor a staff member!").Build());
                        return;
                    }
                    // check if clan is already lvl 2.
                    if (clan.Level >= CLAN_MAX_LEVEL)
                    {
                        await context.Channel.SendMessageAsync("",
                            embed: Utility.ResultFeedback(
                                Utility.RedFailiureEmbed, 
                                Utility.SuccessLevelEmoji[2], 
                                "Clan is already max level. Can't upgrade any further.")
                                .Build());
                        return;
                    }
                    
                    // check available money
                    if (userDb.Money < CLAN_LVLUP_COST)
                    {
                        await context.Channel.SendMessageAsync("",
                            embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"You don't have enough SC to upgrade! It costs {CLAN_LVLUP_COST} SC!").Build());
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
                            $"Successfully upgraded the clan! You can now have {GetMaxUsers(clan)} members!").Build());
                }
                finally
                {
                    lck.Release();
                }

            }
        }

        public async Task RenameClan(SocketCommandContext context, string clanName)
        {
            using (var soraContext = new SoraContext())
            {
                var lck = _coinService.GetOrCreateLock(context.User.Id);
                try
                {
                    if (!await lck.WaitAsync(CoinService.LOCK_TIMOUT_MSECONDS))
                    {
                        await _coinService.LockingErrorMessage(context.Channel);
                        return;
                    }
                    // check if user is in a clan
                    var userDb = Utility.OnlyGetUser(context.User.Id, soraContext);
                    if (userDb == null)
                    {
                        await context.Channel.SendMessageAsync("",
                            embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "You are not in a clan!").Build());
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(userDb.ClanName))
                    {
                        await context.Channel.SendMessageAsync("",
                            embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "You are not in a clan!").Build());
                        return; 
                    }
                    // check if clan still exists
                    var clan = Utility.GetClan(userDb.ClanName, soraContext);
                    if (clan == null)
                    {
                        await context.Channel.SendMessageAsync("",
                            embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Your clan doesn't exist anymore...").Build());
                        return;
                    }
                    // otherwise check if he's owner
                    if (clan.OwnerId != context.User.Id)
                    {
                        await context.Channel.SendMessageAsync("",
                            embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "You are not the owner of this clan!").Build());
                        return;
                    }
                    // check available money
                    if (userDb.Money < CLAN_RENAME_COST)
                    {
                        await context.Channel.SendMessageAsync("",
                            embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"You don't have enough SC to rename! It costs {CLAN_RENAME_COST} SC!").Build());
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
                            $"Successfully renamed the clan to `{clanName}`").Build());
                }
                finally
                {
                    lck.Release();
                }

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
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "No Clans exist yet. Create one!").Build());
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
                    eb.AddField((x) =>
                    {
                        x.IsInline = false;
                        x.Name = $"{(count)}. {clan.Clan.Name}";
                        x.Value = $"{clan.TotalExp} EXP";
                    });
                    count++;
                }
                await context.Channel.SendMessageAsync("", embed: eb.Build());
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
                            $"You are not part of a clan!").Build());
                    return;
                }
                var clan = Utility.GetClan(userDb.ClanName, soraContext);
                if (clan == null)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            $"Your clan doesn't exist anymore. You have now been removed.").Build());
                    userDb.ClanName = null;
                    userDb.ClanStaff = false;
                    await soraContext.SaveChangesAsync();
                    return;
                }
                if (context.User.Id != clan.OwnerId)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            $"You are not the owner of this guild!").Build());
                    return;
                }
                var eb = new EmbedBuilder()
                {
                    Color = Utility.YellowWarningEmbed,
                    Title = Utility.SuccessLevelEmoji[1]+ " Are you sure you want to delete " + clan.Name+"?",
                    Description = "This cannot be undone! All members will be kicked and the entire clan will be erased.\n If you are certain answer with `y` or `yes`. Otherwise just enter anything."
                };
                await context.Channel.SendMessageAsync("", embed: eb.Build());

                var response = await _interactive.NextMessageAsync(context, true, true, TimeSpan.FromSeconds(45));
                if (response == null)
                {
                    await context.Channel.SendMessageAsync("", embed:
                        Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Didn't answer in time ;_;").Build());
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
                        embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0], $"Successfully removed {clan.Name}!").Build());
                }
                else
                {
                    await context.Channel.SendMessageAsync("", embed:
                        Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Didn't answer with y or yes!").Build());
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
                            $"You are not part of a clan!").Build());
                    return;
                }
                var clan = Utility.GetClan(userDb.ClanName, soraContext);
                if (clan == null)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            $"Your clan doesn't exist anymore...").Build());
                    return;
                }
                if (context.User.Id != clan.OwnerId)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            $"You are not the owner of this guild!").Build());
                    return;
                }
                //Get other user
                var kuserDb = Utility.OnlyGetUser(user.Id, soraContext);
                if (string.IsNullOrWhiteSpace(kuserDb?.ClanName))
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            $"{Utility.GiveUsernameDiscrimComb(user)} is not part of a clan!").Build());
                    return;
                }
                //check if same clan
                if (!kuserDb.ClanName.Equals(clan.Name, StringComparison.OrdinalIgnoreCase))
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            $"{Utility.GiveUsernameDiscrimComb(user)} is not part of your clan!").Build());
                    return;
                }
                
                //make owner
                kuserDb.ClanStaff = true;
                clan.OwnerId = user.Id;
                userDb.ClanStaff = true;
                await soraContext.SaveChangesAsync();
                await context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0], $"Successfully set {Utility.GiveUsernameDiscrimComb(user)} as new owner!").Build());
                //Send DM to user who got promoted.
                try
                {
                    await (await user.GetOrCreateDMChannelAsync()).SendMessageAsync("", embed: Utility.ResultFeedback(Utility.BlueInfoEmbed, Utility.SuccessLevelEmoji[3], $"You have been promoted to clan owner!").Build());
                }
                catch (Exception)
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
                            $"You are not part of a clan!").Build());
                    return;
                }
                if (!userDb.ClanStaff)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            $"You have to be a staff member to remove other staff members!").Build());
                    return;
                }
                var clan = Utility.GetClan(userDb.ClanName, soraContext);
                if (clan == null)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            $"Your clan doesn't exist anymore...").Build());
                    return;
                }
                //Get other user
                var kuserDb = Utility.OnlyGetUser(user.Id, soraContext);
                if (string.IsNullOrWhiteSpace(kuserDb?.ClanName))
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            $"{Utility.GiveUsernameDiscrimComb(user)} is not part of a clan!").Build());
                    return;
                }
                //check if same clan
                if (!kuserDb.ClanName.Equals(clan.Name, StringComparison.OrdinalIgnoreCase))
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            $"{Utility.GiveUsernameDiscrimComb(user)} is not part of your clan!").Build());
                    return;
                }
                //check if he's owner
                if (user.Id == clan.OwnerId)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            $"You can't remove staff from the owner!").Build());
                    return;
                }
                //check if already staff
                if (!kuserDb.ClanStaff)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            $"User is not a staff member!").Build());
                    return;  
                }
                //remove staff
                kuserDb.ClanStaff = false;
                await soraContext.SaveChangesAsync();
                await context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0], $"Successfully removed {Utility.GiveUsernameDiscrimComb(user)} from the staff team!").Build());
                //Send DM to user who got promoted.
                try
                {
                    await (await user.GetOrCreateDMChannelAsync()).SendMessageAsync("", embed: Utility.ResultFeedback(Utility.BlueInfoEmbed, Utility.SuccessLevelEmoji[3], $"You have been removed from the staff team!").Build());
                }
                catch (Exception)
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
                            $"You are not part of a clan!").Build());
                    return;
                }
                if (!userDb.ClanStaff)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            $"You have to be a staff member to make other members staff!").Build());
                    return;
                }
                var clan = Utility.GetClan(userDb.ClanName, soraContext);
                if (clan == null)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            $"Your clan doesn't exist anymore...").Build());
                    return;
                }
                //Get other user
                var kuserDb = Utility.OnlyGetUser(user.Id, soraContext);
                if (string.IsNullOrWhiteSpace(kuserDb?.ClanName))
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            $"{Utility.GiveUsernameDiscrimComb(user)} is not part of a clan!").Build());
                    return;
                }
                //check if same clan
                if (!kuserDb.ClanName.Equals(clan.Name, StringComparison.OrdinalIgnoreCase))
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            $"{Utility.GiveUsernameDiscrimComb(user)} is not part of your clan!").Build());
                    return;
                }
                //check if he's owner
                if (user.Id == clan.OwnerId)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            $"Owner is always staff...").Build());
                    return;
                }
                //check if already staff
                if (kuserDb.ClanStaff)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            $"User is a staff member already!").Build());
                    return;  
                }
                //make user staff
                kuserDb.ClanStaff = true;
                await soraContext.SaveChangesAsync();
                await context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0], $"Successfully added {Utility.GiveUsernameDiscrimComb(user)} to the staff team!").Build());
                //Send DM to user who got promoted.
                try
                {
                    await (await user.GetOrCreateDMChannelAsync()).SendMessageAsync("", embed: Utility.ResultFeedback(Utility.BlueInfoEmbed, Utility.SuccessLevelEmoji[3], $"You have been promoted to staff!").Build());
                }
                catch (Exception)
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
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"You are not part of a clan!").Build());
                    return; 
                }
                if (!userDb.ClanStaff)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"You have to be a staff member to kick other users!").Build());
                    return; 
                }
                var clan = Utility.GetClan(userDb.ClanName, soraContext);
                if (clan == null)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"Your clan doesn't exist anymore...").Build());
                    return; 
                }
                //Get other user
                var kuserDb = Utility.OnlyGetUser(user.Id, soraContext);
                if (string.IsNullOrWhiteSpace(kuserDb?.ClanName))
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"{Utility.GiveUsernameDiscrimComb(user)} is not part of a clan!").Build());
                    return; 
                }
                //check if same clan
                if (!kuserDb.ClanName.Equals(clan.Name, StringComparison.OrdinalIgnoreCase))
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"{Utility.GiveUsernameDiscrimComb(user)} is not part of your clan!").Build());
                    return; 
                }
                //check if he's owner
                if (user.Id == clan.OwnerId)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"You cannot kick the owner of the clan!").Build());
                    return; 
                }
                //kick user
                kuserDb.ClanName = null;
                kuserDb.ClanStaff = false;
                await soraContext.SaveChangesAsync();
                await context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0], $"Successfully kicked {Utility.GiveUsernameDiscrimComb(user)}!").Build());
                //Send DM to user who got kicked.
                try
                {
                        await (await user.GetOrCreateDMChannelAsync()).SendMessageAsync("", embed: Utility.ResultFeedback(Utility.BlueInfoEmbed, 
                            Utility.SuccessLevelEmoji[3], $"You have been kicked from {clan.Name}!").Build());
                }
                catch (Exception)
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
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"You are not part of a clan!").Build());
                    return; 
                }
                var clan = Utility.GetClan(userDb.ClanName, soraContext);
                if (clan == null)
                {
                    //CLan doesnt exist anymore so remove his aquintance
                    userDb.ClanName = null;
                    userDb.ClanStaff = false;
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0], $"Successfully left the clan.").Build());
                    await soraContext.SaveChangesAsync();
                    return; 
                }
                if (clan.OwnerId == context.User.Id)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"You cannot leave a clan you own. Transfer ownership or remove it completely!").Build());
                    return; 
                }
                userDb.ClanName = null;
                userDb.ClanStaff = false;
                await context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0], $"Successfully left the clan.").Build());
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
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"You are not part of a clan!").Build());
                    return; 
                }
                if (!userDb.ClanStaff)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"You have to be a staff member to edit the clan description!").Build());
                    return; 
                }
                var clan = Utility.GetClan(userDb.ClanName, soraContext);
                if (clan == null)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"Your clan doesn't exist anymore...").Build());
                    return; 
                }
                clan.Message = clanMessage;
                await soraContext.SaveChangesAsync();
                await context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0], $"Successfully changed clan description!").Build());
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
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"You have no invite from this clan!").Build());
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
                            $"Successfully declined Clan Invite!").Build());
                }
                else
                {
                    //check if already in a clan
                    var userDb = Utility.GetOrCreateUser(context.User.Id, soraContext);
                    if (!string.IsNullOrWhiteSpace(userDb.ClanName))
                    {
                        await context.Channel.SendMessageAsync("",
                            embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                                $"You are already part of a clan! Please leave first before joining another one!").Build());
                        return;
                    }
                    //check if clan still exists.
                    clan = Utility.GetClan(clanName, soraContext);
                    if (clan == null)
                    {
                        soraContext.ClanInvites.Remove(invite);

                        await context.Channel.SendMessageAsync("",
                            embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                                $"Clan does not exist anymore! Invite was removed.").Build());
                        await soraContext.SaveChangesAsync();
                        return;
                    }
                    //CHECK IF CLAN IS FULLL
                    if (clan.Members.Count >= GetMaxUsers(clan))
                    {
                        await context.Channel.SendMessageAsync("",
                            embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                                $"Clan is already full! Wait until someone leaves...").Build());
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
                            "You successfully joined " + clan.Name).Build());
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
                        await (await mod.GetOrCreateDMChannelAsync()).SendMessageAsync("", embed: eb.Build());
                    }
                }
                catch (Exception)
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
                            $"You are not part of a clan!").Build());
                    return;
                }
                if (!userDb.ClanStaff)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            $"You are not a Staff member!").Build());
                    return;
                }
                var clan = Utility.GetClan(userDb.ClanName, soraContext);
                if (clan == null)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            $"Your clan doesn't exist anymore...").Build());
                    return;
                }
                //check if invite exists!
                var invite = soraContext.ClanInvites.FirstOrDefault(x =>
                    x.ClanName.Equals(clan.Name, StringComparison.OrdinalIgnoreCase) && x.UserId == user.Id);
                if (invite == null)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            $"This person has no invite yet!").Build());
                    return;
                }
                //remove invite
                soraContext.ClanInvites.Remove(invite);
                await soraContext.SaveChangesAsync();
                await context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0],
                        $"The invite has been revoked!").WithDescription("No DM will be sent to the user in this case.").Build());
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
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"You are not part of a clan!").Build());
                    return; 
                }
                if (!userDb.ClanStaff)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"You are not a Staff member!").Build());
                    return; 
                }
                var clan = Utility.GetClan(userDb.ClanName, soraContext);
                if (clan == null)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"Your clan doesn't exist anymore...").Build());
                    return; 
                }
                //check if invite already exists!
                if (soraContext.ClanInvites.Any(x =>
                    x.ClanName.Equals(clan.Name, StringComparison.OrdinalIgnoreCase) && x.UserId == user.Id))
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"You already invited this person!").Build());
                    return; 
                }
                if (clan.Members.Count >= GetMaxUsers(clan))
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"You already reached the max amount of members!").Build());
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
                    await (await user.GetOrCreateDMChannelAsync()).SendMessageAsync("", embed: eb.Build());
                }
                catch (Exception)
                {
                    send = false;
                }
                var eb2 = Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0],
                    "User successfully invited!");
                if (!send)
                    eb2.Description = "Failed to send Clan Invite via DM! Please contact the user directly!";
                await context.Channel.SendMessageAsync("", embed: eb2.Build());
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
                            $"You are not part of a clan!").Build());
                    return;
                }
                if (!userDb.ClanStaff)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            $"You must be a staff member to change the clan avatar!").Build());
                    return;
                }
                //Get Clan
                var clan = Utility.GetClan(userDb.ClanName, soraContext);
                if (clan == null)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            $"Your clan doesn't exist anymore :/").Build());
                    return;
                }
                
                clan.HasImage = false;
                await soraContext.SaveChangesAsync();
                await context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0],
                        "Successfully removed clan avatar!").Build());
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
                            $"You are not part of a clan!").Build());
                    return;
                }
                if (!userDb.ClanStaff)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            $"You must be a staff member to change the clan avatar!").Build());
                    return;
                }
                //Get Clan
                var clan = Utility.GetClan(userDb.ClanName, soraContext);
                if (clan == null)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                            $"Your clan doesn't exist anymore :/").Build());
                    return;
                }
                
                clan.HasImage = true;
                clan.AvatarUrl = url;
                await soraContext.SaveChangesAsync();
                await context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0],
                        "Successfully set new clan avatar!").Build());
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
                            embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"You are not in a clan! Enter a clan name!").Build());
                        return;
                    }
                    clanName = userDb.ClanName;
                }
                var clan = Utility.GetClan(clanName, soraContext);
                if (clan == null)
                {
                    await context.Channel.SendMessageAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"Clan doesn't exist!").Build());
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
                bool overflow = members.Count > 25;
                int count =  overflow ? 22 : members.Count;
                for (int i = 0; i < count; i++)
                {
                    var mbm = members[i];
                    IUser user = context.Client.GetUser(mbm.UserId) ?? await _restClient.GetUserAsync(mbm.UserId) as IUser;
                    var userName = (user == null ? mbm.UserId.ToString() : Utility.GiveUsernameDiscrimComb(user));
                    eb.AddField(x =>
                    {
                        x.IsInline = true;
                        x.Name = $"{i + 1}. {(mbm.ClanStaff ? "[S] " : "")}{userName}";
                        x.Value =
                            $"Lvl. {ExpService.CalculateLevel(mbm.Exp)} \tEXP: {mbm.Exp}\n*Joined: {(mbm.JoinedClan.ToString("dd/MM/yyyy"))}*";
                    });
                }
                if (!overflow)
                {
                    await context.Channel.SendMessageAsync("", embed: eb.Build());
                    return;                    
                }
                // now lets add the overflow. 2 more fields to add
                // all others. 22 normal of a max of 35 -> 13, 7,6
                bool two = members.Count > 29;
                string first = "";
                for (int i = 22; i < (two ? 29 : members.Count); i++)
                {
                    var mbm = members[i];
                    IUser user = context.Client.GetUser(mbm.UserId) ?? await _restClient.GetUserAsync(mbm.UserId) as IUser;
                    var userName = (user == null ? mbm.UserId.ToString() : Utility.GiveUsernameDiscrimComb(user));
                    first += $"**{i + 1}. {(mbm.ClanStaff ? "[S] " : "")}{userName}**\n" +
                             $"*Joined: {(mbm.JoinedClan.ToString("dd/MM/yyyy"))}*\n";
                }
                eb.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = $"More members";
                    x.Value = first;
                });
                if (!two)
                {
                    await context.Channel.SendMessageAsync("", embed: eb.Build());
                    return;   
                }
                string second = "";
                for (int i = 29; i < members.Count; i++)
                {
                    var mbm = members[i];
                    IUser user = context.Client.GetUser(mbm.UserId) ?? await _restClient.GetUserAsync(mbm.UserId) as IUser;
                    var userName = (user == null ? mbm.UserId.ToString() : Utility.GiveUsernameDiscrimComb(user));
                    second += $"**{i + 1}. {(mbm.ClanStaff ? "[S] " : "")}{userName}**\n" +
                             $"*Joined: {(mbm.JoinedClan.ToString("dd/MM/yyyy"))}*\n";
                }
                eb.AddField(x =>
                {
                    x.IsInline = true;
                    x.Name = $"Even more members";
                    x.Value = second;
                });
                await context.Channel.SendMessageAsync("", embed: eb.Build());
            }
        }

        public async Task CreateClan(SocketCommandContext context, string clanName)
        {
            using (var soraContext = new SoraContext())
            {
                var lck = _coinService.GetOrCreateLock(context.User.Id);
                try 
                {
                    if (!await lck.WaitAsync(CoinService.LOCK_TIMOUT_MSECONDS))
                    {
                        await _coinService.LockingErrorMessage(context.Channel);
                        return;
                    }
                    //Check User Criterias
                    var userDb = Utility.OnlyGetUser(context.User.Id, soraContext);
                    if (userDb == null)
                    {
                        await context.Channel.SendMessageAsync("",
                            embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"You need to be at least level {MINIMUM_CREATE_LEVEL} to create a clan!").Build());
                        return;
                    }
                    if (!string.IsNullOrWhiteSpace(userDb.ClanName))
                    {
                        await context.Channel.SendMessageAsync("",
                            embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"You are already in a clan! Please leave that clan first!")
                                .WithDescription("If you are the owner, you can pass on the clan to someone else before leaving.").Build());
                        return;
                    }
                    if (ExpService.CalculateLevel(userDb.Exp) < MINIMUM_CREATE_LEVEL)
                    {
                        await context.Channel.SendMessageAsync("",
                            embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"You need to be at least level {MINIMUM_CREATE_LEVEL} to create a clan!").Build());
                        return;
                    }
                    
                    // check his cash
                    if (userDb.Money < CLAN_CREATION_COST)
                    {
                        await context.Channel.SendMessageAsync("",
                            embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"Creating a clan costs {CLAN_CREATION_COST} Sora Coins! You don't have enough!").Build());
                        return;
                    }
                    
                    //check if clan already exists
                    if (soraContext.Clans.Any(x => x.Name.Equals(clanName, StringComparison.OrdinalIgnoreCase)))
                    {
                        await context.Channel.SendMessageAsync("",
                            embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Clan Name is already taken!").Build());
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
                        embed: Utility.ResultFeedback(Utility.GreenSuccessEmbed, Utility.SuccessLevelEmoji[0], $"Clan \"{clanName}\" has been created!").Build());
                
                }
                finally
                {
                    lck.Release();
                }
            }
        }
    }
}