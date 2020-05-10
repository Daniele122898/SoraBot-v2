using System;
using System.Threading.Tasks;
using Discord;
using Microsoft.Extensions.Logging;

namespace SoraBot.Common.Extensions.Modules
{
    public static class DNetExtensions
    {
        public static async Task<bool> TryAddRoleAsync(this IGuildUser user, IRole role, ILogger log = null)
        {
            try
            {
                await user.AddRoleAsync(role).ConfigureAwait(false);
                return true;
            }
            catch (Exception e)
            {
                log?.LogWarning(e, $"Failed to assign role {role.Name} to user {user.Username}.");
                return false;
            }
        }
        
        public static async Task<bool> TryRemoveRoleAsync(this IGuildUser user, IRole role, ILogger log = null)
        {
            try
            {
                await user.RemoveRoleAsync(role).ConfigureAwait(false);
                return true;
            }
            catch (Exception e)
            {
                log?.LogWarning(e, $"Failed to remove role {role.Name} from user {user.Username}.");
                return false;
            }
        }
    }
}