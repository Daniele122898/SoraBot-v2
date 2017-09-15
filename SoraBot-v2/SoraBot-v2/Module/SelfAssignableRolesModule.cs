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
        public async Task AddDefaultRole([Remainder] string roleName)
        {
            await _sarService.AddDefaultRole(Context, roleName.Trim());
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

        [Command("iamnot"), Alias("rmsar", "rsar", "nsar"), Summary("Removes specified self assignable role from you")]
        public async Task IAmNotSar([Summary("Role name"), Remainder] string roleName)
        {
            await _sarService.IAmNotSar(Context, roleName.Trim());
        }

        [Command("sarlist"), Alias("getlist", "sars", "lsar"), Summary("Gives you a list of all self-asisgnable roles")]
        public async Task Sars()
        {
            await _sarService.ListSars(Context);
        }
        
    }
}