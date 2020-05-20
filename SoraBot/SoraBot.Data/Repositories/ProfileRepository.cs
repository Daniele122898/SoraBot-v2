using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArgonautCore.Lw;
using Microsoft.EntityFrameworkCore;
using SoraBot.Data.Dtos.Profile;
using SoraBot.Data.Extensions;
using SoraBot.Data.Models.SoraDb;
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
        
        public async Task<Option<ProfileImageGenDto>> GetProfileStatistics(ulong userId, ulong guildId)
        {
            return await _soraTransactor.DoAsync<Option<ProfileImageGenDto>>(async context =>
            {
                var user = await context.Users.FindAsync(userId).ConfigureAwait(false);
                if (user == null) return Option.None<ProfileImageGenDto>();

                var globalRank = await context.Users
                    .Where(u => u.Exp > user.Exp)
                    .CountAsync()
                    .ConfigureAwait(false);

                var guildUser = await context.GuildUsers
                    .FirstOrDefaultAsync(x => x.UserId == userId && x.GuildId == guildId)
                    .ConfigureAwait(false) ?? new GuildUser(0,0,0); // Just so we have default values to work with

                var localRank = await context.GuildUsers
                    .Where(g => g.Exp > guildUser.Exp)
                    .CountAsync()
                    .ConfigureAwait(false);
                    
                return new ProfileImageGenDto()
                {
                    GlobalExp = user.Exp,
                    GlobalRank = globalRank + 1,
                    HasCustomBg = user.HasCustomProfileBg,
                    LocalExp = guildUser.Exp,
                    LocalRank = localRank + 1
                };
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

        public async Task<Option<List<User>>> GetTop150Users()
            => await _soraTransactor.DoAsync<Option<List<User>>>(async context =>
            {
                var users = await context.Users
                    .OrderByDescending(x => x.Exp)
                    .Take(150)
                    .ToListAsync()
                    .ConfigureAwait(false);
                if (users.Count == 0)
                    return Option.None<List<User>>();
                return users;
            }).ConfigureAwait(false);
    }
}