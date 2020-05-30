using System.Threading.Tasks;
using ArgonautCore.Lw;
using Discord;

namespace SoraBot.Services.Users
{
    public interface IUserService
    {
        /// <summary>
        /// Returns User if found in discord or custom cache. Otherwise returns null
        /// </summary>
        Option<IUser> Get(ulong id);

        /// <summary>
        /// Returns User if found in discord or custom cache. Otherwise returns null
        /// </summary>
        Option<IGuildUser> Get(ulong userId, ulong guildId);
        
        /// <summary>
        /// First check's Discord cache, then  custom cache and finally if all are null it will
        /// try to fetch the user from the rest client. Returns null only if failed to fetch
        /// => User probably doesn't exist in our reach.
        /// </summary>
        Task<Option<IUser>> GetOrSetAndGet(ulong id);

        Task<Option<IGuildUser>> GetOrSetAndGet(ulong userId, ulong guildId);
    }
}