using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using SoraBot_v2.Data;
using SoraBot_v2.Services;

namespace SoraBot_v2.Module
{
    [Name("Clan")]
    public class ClanModule : ModuleBase<SocketCommandContext>
    {
        private ClanService _clanService;

        public ClanModule(ClanService clanService)
        {
            _clanService = clanService;
        }

        [Command("clevelup", RunMode = RunMode.Async), Alias("cupgrade", "upgradeclan", "memberlimit"),
         Summary("Upgrade clan to increase member limit by 5. Costs 7'500 SC")]
        public async Task UpgradeClan()
        {
            await _clanService.LevelUpClan(Context);
        }

        [Command("crename", RunMode = RunMode.Async), Alias("renameclan", "clanrename"),
         Summary("To rename your clan.")]
        public async Task RenameClan([Remainder] string clanName)
        {
            if (Context.Message.Content.Contains("<") && Context.Message.Content.Contains(">"))
            {
                await Context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Please don't include any emotes! Anything with `<` and `>`").Build());
                return;
            }
            if (clanName.Length > 20)
            {
                await Context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Clan Name should not exceed 20 characters!").Build());
                return;
            }
            if (clanName.Length < 3)
            {
                await Context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Clan Name should at least have 3 characters!").Build());
                return;
            }
            await _clanService.RenameClan(Context, clanName);
        }

        [Command("createclan", RunMode = RunMode.Async), Alias("cclan"), Summary("Create a clan. Costs 2000 SC.")]
        public async Task CreateClan([Remainder] string clanName)
        {
            if (Context.Message.Content.Contains("<") && Context.Message.Content.Contains(">"))
            {
                await Context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Please don't include any emotes! Anything with `<` and `>`").Build());
                return;
            }
            if (clanName.Length > 20)
            {
                await Context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Clan Name should not exceed 20 characters!").Build());
                return;
            }
            if (clanName.Length < 3)
            {
                await Context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Clan Name should at least have 3 characters!").Build());
                return;
            }
            await _clanService.CreateClan(Context, clanName.Trim());
        }

        [Command("claninfo"), Alias("cinfo"), Summary("Shows info about clan")]
        public async Task ClanInfo([Remainder] string clanName ="")
        {
            await _clanService.ShowClanInfo(Context, clanName.Trim());
        }
        
        
        [Command("claninfo"), Alias("cinfo"), Summary("Shows info about clan")]
        public async Task ClanInfo(SocketUser user)
        {
            using (var soraContext = new SoraContext())
            {
                var userdb = Utility.GetOrCreateUser(user.Id, soraContext);
                if (string.IsNullOrWhiteSpace(userdb.ClanName))
                {
                    await ReplyAsync("", embed: Utility.ResultFeedback(
                        Utility.RedFailiureEmbed,
                        Utility.SuccessLevelEmoji[2],
                        $"{Utility.GiveUsernameDiscrimComb(user)} is in no clan!").Build());
                    return;
                }
                // else get clan info
                await _clanService.ShowClanInfo(Context, userdb.ClanName);
            }
        }

        [Command("rmclanavatar"), Alias("rmcavatar", "rmcava"), Summary("Remove Clan Avatar")]
        public async Task RmClanAvatar()
        {
            await _clanService.RmClanAvatar(Context);
        }

        [Command("clanavatar"), Alias("cavatar", "cava"), Summary("Set Clan Avatar")]
        public async Task ClanAvatar([Remainder] string url ="")
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                if (Context.Message.Attachments.Count < 1)
                {
                    await ReplyAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "If you do not specify a link to an Image then please attach one!").Build());
                    return;
                }
                else if (Context.Message.Attachments.Count > 1)
                {
                    await ReplyAsync("",
                        embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "Please only attach one Image!").Build());
                    return;                   
                }
                url = Context.Message.Attachments.First().Url;
            }
            if (!url.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) && !url.EndsWith(".png", StringComparison.OrdinalIgnoreCase) && !url.EndsWith(".gif", StringComparison.OrdinalIgnoreCase) && !url.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
            {
                await Context.Channel.SendMessageAsync("", embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], "You must link or attach an Image!").Build());
                return;
            }
            await _clanService.SetClanAvatar(Context, url);
        }

        [Command("clanleave"), Alias("cleave", "leaveclan"), Summary("Leave your clan")]
        public async Task LeaveClan()
        {
            await _clanService.LeaveClan(Context);
        }
        
        [Command("rmclaninvite"), Alias("rmcinvite", "rminvite", "revoke"), Summary("Revoke invite to a User")]
        public async Task RmClanInvite(SocketGuildUser user) 
        {
            await _clanService.RevokeInvite(Context, user);
        }
        
        [Command("rmclaninvite"), Alias("rmcinvite", "rminvite", "revoke"), Summary("Revoke invite to a User")]
        public async Task RmClanInvite([Remainder] string name) 
        {
            name = name.Trim();
            var userName = name.Split("#");
            if (userName.Length < 2)
            {
                await Context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"Invalid Username! Please use: Name#Discrim").Build());
                return; 
            }
            var user = Context.Client.GetUser(userName[0], userName[1]);
            if (user == null)
            {
                await Context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"Couldn't find user "+name).Build());
                return;  
            }
            await _clanService.RevokeInvite(Context, user);
        }
        
        [Command("removeclanstaff"), Alias("rmcstaff"), Summary("Remove Staff")]
        public async Task RemoveStaff(SocketGuildUser user)
        {
            await _clanService.RemoveStaff(Context, user);
        }
        
        [Command("removeclanstaff"), Alias("rmcstaff"), Summary("Remove Staff")]
        public async Task RemoveStaff([Remainder] string name)
        {
            name = name.Trim();
            var userName = name.Split("#");
            if (userName.Length < 2)
            {
                await Context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"Invalid Username! Please use: Name#Discrim").Build());
                return; 
            }
            var user = Context.Client.GetUser(userName[0], userName[1]);
            if (user == null)
            {
                await Context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"Couldn't find user "+name).Build());
                return;  
            }
            await _clanService.RemoveStaff(Context, user);
        }

        [Command("clanremove", RunMode = RunMode.Async), Alias("cremove", "removeclan", "rmc", "rmclan")]
        public async Task RemoveClan()
        {
            await _clanService.RemoveClan(Context);
        }
        
        [Command("clanowner"), Alias("cowner"), Summary("Make user Owner")]
        public async Task MakeOwner(SocketGuildUser user)
        {
            await _clanService.MakeOwner(Context, user);
        }
        
        [Command("clanowner"), Alias("cowner"), Summary("Make user Owner")]
        public async Task MakeOwner([Remainder] string name)
        {
            name = name.Trim();
            var userName = name.Split("#");
            if (userName.Length < 2)
            {
                await Context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"Invalid Username! Please use: Name#Discrim").Build());
                return; 
            }
            var user = Context.Client.GetUser(userName[0], userName[1]);
            if (user == null)
            {
                await Context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"Couldn't find user "+name).Build());
                return;  
            }
            await _clanService.MakeOwner(Context, user);
        }

        [Command("clanstaff"), Alias("cstaff"), Summary("Make user Staff")]
        public async Task MakeStaff(SocketGuildUser user)
        {
            await _clanService.MakeStaff(Context, user);
        }
        
        [Command("clanstaff"), Alias("cstaff"), Summary("Make user Staff")]
        public async Task MakeStaff([Remainder] string name)
        {
            name = name.Trim();
            var userName = name.Split("#");
            if (userName.Length < 2)
            {
                await Context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"Invalid Username! Please use: Name#Discrim").Build());
                return; 
            }
            var user = Context.Client.GetUser(userName[0], userName[1]);
            if (user == null)
            {
                await Context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"Couldn't find user "+name).Build());
                return;  
            }
            await _clanService.MakeStaff(Context, user);
        }

        [Command("clankick"), Alias("ckick"), Summary("Kick user")]
        public async Task KickUser(SocketGuildUser user)
        {
            await _clanService.KickUser(Context, user);
        }
        
        [Command("clankick"), Alias("ckick"), Summary("Kick user")]
        public async Task KickUser([Remainder] string name)
        {
            name = name.Trim();
            var userName = name.Split("#");
            if (userName.Length < 2)
            {
                await Context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"Invalid Username! Please use: Name#Discrim").Build());
                return; 
            }
            var user = Context.Client.GetUser(userName[0], userName[1]);
            if (user == null)
            {
                await Context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"Couldn't find user "+name).Build());
                return;  
            }
            await _clanService.KickUser(Context, user);
        }
        
        [Command("claninvite", RunMode = RunMode.Async), Alias("cinvite"), Summary("Invite a User")]
        public async Task ClanInvite(SocketGuildUser user)
        {
            await _clanService.InviteUser(Context, user);
        }
        
        
        [Command("claninvite", RunMode = RunMode.Async), Alias("cinvite"), Summary("Invite a User")]
        public async Task ClanInvite([Remainder] string name)
        {
            name = name.Trim();
            var userName = name.Split("#");
            if (userName.Length < 2)
            {
                await Context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"Invalid Username! Please use: Name#Discrim").Build());
                return; 
            }
            var user = Context.Client.GetUser(userName[0], userName[1]);
            if (user == null)
            {
                await Context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"Couldn't find user "+name).Build());
                return;  
            }
            await _clanService.InviteUser(Context, user);
        }
        
        [Command("clandescription"), Alias("cdescription", "cdesc", "clandesc"), Summary("Change clan description")]
        public async Task ClanDescription([Remainder]string description)
        {
            description = description.Trim();
            if (string.IsNullOrWhiteSpace(description))
            {
                await Context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"Description must have... at least something in it.").Build());
                return; 
            }
            if (description.Length > 1500)
            {
                await Context.Channel.SendMessageAsync("",
                    embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2], $"Description should not be longer than 1500 characters!").Build());
                return; 
            }
            await _clanService.EditClanDescription(Context, description);
        }
        
        [Command("clanaccept"), Alias("caccept", "accept"), Summary("Accept Clan Invite")]
        public async Task AcceptInvite([Remainder] string clanName)
        {
            await _clanService.EditClanInvite(Context, clanName, true);
        }
        
        [Command("clandecline"), Alias("cdecline", "decline"), Summary("Decline Clan Invite")]
        public async Task DeclineInvite([Remainder] string clanName)
        {
            await _clanService.EditClanInvite(Context, clanName, false);
        }

        [Command("clanlist", RunMode = RunMode.Async), Alias("clantop", "clantop10", "top10clans", "bestclans", "clist", "clans"),
         Summary("Shows all the clans in a list")]
        public async Task ClanList()
        {
            await _clanService.ShowClanList(Context);
        }
    }
}