using System.Threading.Tasks;
using ArgonautCore.Lw;
using SoraBot.Data.Extensions;
using SoraBot.Data.Models.SoraDb;
using SoraBot.Data.Repositories.Interfaces;

namespace SoraBot.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ITransactor<SoraContext> _soraTransactor;

        public UserRepository(ITransactor<SoraContext> soraTransactor)
        {
            _soraTransactor = soraTransactor;
        }

        public async Task<Some<User>> GetOrCreateUser(ulong id)
        {
            return await _soraTransactor.DoInTransactionAndGetAsync(async context =>
            {
                var user = await context.Users.FindAsync(id).ConfigureAwait(false);
                if (user != null)
                    return  user;

                // Otherwise we'll have to create the user ourselves
                user = new User(){Id = id};
                await context.Users.AddAsync(user).ConfigureAwait(false);
                await context.SaveChangesAsync().ConfigureAwait(false);
                return user;
            }).ConfigureAwait(false);
        }

        public async Task<User> GetUser(ulong id)
        {
            return await _soraTransactor.ReadUncommittedAsync(async context =>
                await context.Users.FindAsync(id).ConfigureAwait(false));
        }

        public async Task TryAddUserExp(ulong userId, uint expToAdd)
        {
            await _soraTransactor.DoInTransactionAsync(async context =>
            {
                var user = await context.Users.GetOrCreateUserNoSaveAsync(userId);
                user.Exp += expToAdd;
                await context.SaveChangesAsync().ConfigureAwait(false);
            }).ConfigureAwait(false);
        }
    }
}