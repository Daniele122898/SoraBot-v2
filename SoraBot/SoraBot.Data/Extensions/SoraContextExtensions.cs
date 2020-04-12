using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SoraBot.Data.Models.SoraDb;

namespace SoraBot.Data.Extensions
{
    public static class SoraContextExtensions
    {
        public static async Task<User> GetOrCreateUserNoSaveAsync(this DbSet<User> users, ulong userId)
        {
            var user = await users.FindAsync(userId).ConfigureAwait(false);
            if (user != null)
                return user;
            
            // Otherwise we'll have to create the user ourselves
            user = new User(){Id = userId};
            await users.AddAsync(user);
            return user;
        }
    }
}