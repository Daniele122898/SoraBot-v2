using System.Linq;
using System.Threading.Tasks;
using ArgonautCore.Maybe;
using Microsoft.EntityFrameworkCore;
using SoraBot.Data.Dtos.Profile;
using SoraBot.Data.Repositories.Interfaces;

namespace SoraBot.Data.Repositories
{
    public class ProfileRepository : IProfileRepository
    {
        private readonly ITransactor<SoraContext> _soraTransactor;

        public ProfileRepository(ITransactor<SoraContext> soraTransactor)
        {
            _soraTransactor = soraTransactor;
        }
        
        public async Task<Maybe<ProfileImageGenDto>> GetProfileStatistics(ulong userId, ulong guildId)
        {
            return await _soraTransactor.DoAsync(async context =>
            {
                var user = await context.Users.FindAsync(userId).ConfigureAwait(false);
                if (user == null) return Maybe.Zero<ProfileImageGenDto>();

                var rank = await context.Users
                    .Where(u => u.Exp > user.Exp)
                    .CountAsync();

                return Maybe.FromVal(new ProfileImageGenDto()
                {
                    GlobalExp = user.Exp,
                    GlobalRank = rank + 1,
                    HasCustomBg = user.HasCustomProfileBg
                });
            }).ConfigureAwait(false);
        }
    }
}