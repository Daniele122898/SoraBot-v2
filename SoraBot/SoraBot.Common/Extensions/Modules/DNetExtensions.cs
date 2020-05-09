using System;
using System.Threading.Tasks;
using Discord;

namespace SoraBot.Common.Extensions.Modules
{
    public static class DNetExtensions
    {
        public static async Task<bool> TryAddRoleAsync(this IGuildUser user, IRole role)
        {
            try
            {
                await user.AddRoleAsync(role).ConfigureAwait(false);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        
        public static async Task<bool> TryRemoveRoleAsync(this IGuildUser user, IRole role)
        {
            try
            {
                await user.RemoveRoleAsync(role).ConfigureAwait(false);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}