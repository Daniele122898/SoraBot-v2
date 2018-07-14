using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord.Commands;
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

        [Command("addsar"), Alias("asar", "addrole"),
         Summary("Adds a self assignable role to the list. If it doesn't exist sora will create it")]
        public async Task AddSar([Remainder] string roleName)
        {
            await _sarService.AddSarToList(Context, roleName.Trim()); //TODO EXPAND THIS
        }

        [Command("defaultrole"), Alias("drole", "default"), Summary("Sets a default role for when users join")]
        public async Task AddDefaultRole([Remainder] string roleName, int cost = 0, [Remainder] string expires = null)
        {
            if (cost == 0)
            {
                await _sarService.AddDefaultRole(Context, roleName.Trim());
                return;
            }
            
            // parse time
            
            
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

        [Command("iam"), Alias("sar"), Summary("Assigns the role to you if it exists")]
        public async Task IAmSar([Summary("Role name"),Remainder] string roleName)
        {
            await _sarService.IAmSar(Context, roleName.Trim());
        }

        [Command("iamnot"), Summary("Removes specified self assignable role from you")]
        public async Task IAmNotSar([Summary("Role name"), Remainder] string roleName)
        {
            await _sarService.IAmNotSar(Context, roleName.Trim());
        }

        [Command("sarlist"), Alias("getlist", "sars", "lsar"), Summary("Gives you a list of all self-asisgnable roles")]
        public async Task Sars()
        {
            await _sarService.ListSars(Context);
        }

        private double GetTime(string msg)
        {
            var regex = Regex.Matches(msg, @"(\d+)\s{0,1}([a-zA-Z]*)");
            double timeToAdd = 0;
            for (int i = 0; i < regex.Count; i++)
            {
                var captures = regex[i].Groups;
                if (captures.Count < 3)
                {
                    Console.WriteLine("CAPTURES COUNT LESS THEN 3");
                    return 0;
                }

                double amount = 0;

                if (!Double.TryParse(captures[1].ToString(), out amount))
                {
                    Console.WriteLine($"COULDNT PARSE DOUBLE : {captures[1].ToString()}");
                    return 0;
                }

                switch (captures[2].ToString())
                {
                    case ("weeks"):
                    case ("week"):
                    case ("w"):
                        timeToAdd += amount * 604800;
                        break;
                    case ("day"):
                    case ("days"):
                    case ("d"):
                        timeToAdd += amount * 86400;
                        break;
                    case ("hours"):
                    case ("hour"):
                    case ("h"):
                        timeToAdd += amount * 3600;
                        break;
                    case ("minutes"):
                    case ("minute"):
                    case ("m"):
                    case ("min"):
                    case ("mins"):
                        timeToAdd += amount * 60;
                        break;
                    case ("seconds"):
                    case ("second"):
                    case ("s"):
                        timeToAdd += amount;
                        break;
                    default:
                        Console.WriteLine("SWITCH FAILED");
                        return 0;
                }
            }
            return timeToAdd;
        }
        
    }
}