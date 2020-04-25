using System.Linq;
using System.Threading.Tasks;
using ArgonautCore.Maybe;
using Microsoft.EntityFrameworkCore;
using SoraBot.Data.Dtos.Profile;
using SoraBot.Data.Extensions;
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

        public async Task SetUserHasBgBoolean(ulong userId, bool hasCustomBg)
        {
            await _soraTransactor.DoInTransactionAsync(async context =>
            {
                var user = await context.Users.GetOrCreateUserNoSaveAsync(userId).ConfigureAwait(false);
                user.HasCustomProfileBg = hasCustomBg;
                await context.SaveChangesAsync().ConfigureAwait(false);
            }).ConfigureAwait(false);
        }
    }
}