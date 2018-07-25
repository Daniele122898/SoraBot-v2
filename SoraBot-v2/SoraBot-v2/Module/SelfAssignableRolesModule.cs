using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Humanizer;
using Humanizer.Localisation;
using SoraBot_v2.Data;
using SoraBot_v2.Services;

namespace SoraBot_v2.Module
{
    public class SelfAssignableRolesModule : ModuleBase<SocketCommandContext>
    {
        private readonly SelfAssignableRolesService _sarService;

        public SelfAssignableRolesModule(SelfAssignableRolesService service)
        {
            _sarService = service;
        }

        [Command("addsar", RunMode = RunMode.Async), Alias("asar", "addrole"),
         Summary("Adds a self assignable role to the list. If it doesn't exist sora will create it")]
        public async Task AddSar(string roleName, int cost = 0, [Remainder] string expires = null)
        {
            if (cost == 0)
            {
                Console.WriteLine("No additional info");
                await _sarService.AddSarToList(Context, roleName.Trim());
                return;
            }

            if (string.IsNullOrWhiteSpace(expires))
            {
                Console.WriteLine($"Cost but no time. COST: {cost}");
                await _sarService.AddSarToList(Context, roleName.Trim(), false, cost);
                return;
            }
            
            // parse time
            var time = GetTime(expires);
            // check if time was valid
            if (time == null)
            {
                await ReplyAsync("",
                    embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                        "Failed to parse time for expiration"));
                return;
            }
            TimeSpan timeSpan = new TimeSpan(time[0], time[1], time[2], time[3]);
            
            if (timeSpan.TotalMinutes < 10)
            {
                await ReplyAsync("",
                    embed: Utility.ResultFeedback(Utility.RedFailiureEmbed, Utility.SuccessLevelEmoji[2],
                        "The role must be valid for at least 10 minutes!"));
                return;
            }
            
            Console.WriteLine($"Cost: {cost}, time: {time.ToString()}, timespan: {timeSpan.Humanize(2, maxUnit: TimeUnit.Day, minUnit: TimeUnit.Second, countEmptyUnits:true)}");
            
            await _sarService.AddSarToList(Context, roleName.Trim(), true, cost, timeSpan);
        }

        [Command("defaultrole"), Alias("drole", "default"), Summary("Sets a default role for when users join")]
        public async Task AddDefaultRole([Remainder] string roleName)
        {
                await _sarService.AddDefaultRole(Context, roleName.Trim());
                return;
        }

        [Command("expiring"), Alias("expires"), Summary("Shows all your expiring sars")]
        public async Task ShowExpiringSars()
        {
            using (var soraContext = new SoraContext())
            {
                var exp = soraContext.ExpiringRoles.Where(x => x.UserForeignId == Context.User.Id && x.GuildForeignId == Context.Guild.Id).ToList();
                if (exp.Count == 0)
                {
                    await ReplyAsync("", embed: Utility.ResultFeedback(
                        Utility.RedFailiureEmbed,
                        Utility.SuccessLevelEmoji[2],
                        "You don't have any expiring roles in this Guild!"));
                    return;
                }
                var eb = new EmbedBuilder()
                {
                    Color = Utility.PurpleEmbed,
                    ThumbnailUrl = Context.Guild.IconUrl ?? Utility.StandardDiscordAvatar,
                    Author = new EmbedAuthorBuilder()
                    {
                        IconUrl = Context.User.GetAvatarUrl() ?? Utility.StandardDiscordAvatar,
                        Name = Utility.GiveUsernameDiscrimComb(Context.User)
                    },
                    Title = $"Your Expiring Roles in {Context.Guild.Name}",
                };
                foreach (var role in exp)
                {
                    var r = Context.Guild.GetRole(role.RoleForeignId);
                    if(r== null)
                        continue;
                    eb.AddField(x =>
                    {
                        x.IsInline = false;
                        x.Name = r.Name;
                        x.Value = $"Expires in: {role.ExpiresAt.Subtract(DateTime.UtcNow).Humanize(2, maxUnit: TimeUnit.Day, minUnit: TimeUnit.Second, countEmptyUnits:true)}";
                    });
                }
                await ReplyAsync("", embed: eb);
            }
        }

        [Command("toggledefault"), Alias("toggledef"), Summary("Toggles if default role is on or off")]
        public async Task ToggleDefault()
        {
            await _sarService.ToggleDefaultRole(Context);
        }

        [Command("rmsar"), Alias("rsar", "rmrole", "delrole"), Summary("Removes a self-assignable role")]
        public async Task RemoveSar([Remainder] string roleName)
        {
            await _sarService.RemoveSarFromList(Context, roleName.Trim());
        }

        [Command("iam", RunMode = RunMode.Async), Alias("sar"), Summary("Assigns the role to you if it exists")]
        public async Task IAmSar([Summary("Role name"),Remainder] string roleName)
        {
            await _sarService.IAmSar(Context, roleName.Trim());
        }

        [Command("iamnot", RunMode = RunMode.Async), Summary("Removes specified self assignable role from you")]
        public async Task IAmNotSar([Summary("Role name"), Remainder] string roleName)
        {
            await _sarService.IAmNotSar(Context, roleName.Trim());
        }

        [Command("sarlist"), Alias("getlist", "sars", "lsar"), Summary("Gives you a list of all self-asisgnable roles")]
        public async Task Sars()
        {
            await _sarService.ListSars(Context);
        }

        private int[] GetTime(string msg)
        {
            var regex = Regex.Matches(msg, @"(\d+)\s{0,1}([a-zA-Z]*)");
            var add = new int[4];
            for (int i = 0; i < regex.Count; i++)
            {
                var captures = regex[i].Groups;
                if (captures.Count < 3)
                {
                    Console.WriteLine("CAPTURES COUNT LESS THEN 3");
                    return null;
                }

                int amount = 0;

                if (!int.TryParse(captures[1].ToString(), out amount))
                {
                    Console.WriteLine($"COULDNT PARSE DOUBLE : {captures[1].ToString()}");
                    return null;
                }

                switch (captures[2].ToString())
                {
                    case ("weeks"):
                    case ("week"):
                    case ("w"):
                        add[0] += amount * 7;
                        break;
                    case ("day"):
                    case ("days"):
                    case ("d"):
                        add[0] += amount;
                        break;
                    case ("hours"):
                    case ("hour"):
                    case ("h"):
                        add[1] += amount;
                        break;
                    case ("minutes"):
                    case ("minute"):
                    case ("m"):
                    case ("min"):
                    case ("mins"):
                        add[2] += amount;
                        break;
                    case ("seconds"):
                    case ("second"):
                    case ("secs"):
                    case ("sec"):
                    case ("s"):
                        add[3] += amount;
                        break;
                    default:
                        Console.WriteLine("SWITCH FAILED");
                        return null;
                }
            }
            return add;
        }
        
    }
}