using System.Linq;
using System.Threading.Tasks;
using SoraBot.Data.Extensions;
using SoraBot.Data.Repositories.Interfaces;

namespace SoraBot.Data.Repositories
{
    public class CoinRepository : ICoinRepository
    {
        private readonly ITransactor<SoraContext> _soraTransactor;

        public CoinRepository(ITransactor<SoraContext> soraTransactor)
        {
            _soraTransactor = soraTransactor;
        }
        
        public async Task GiveAmount(ulong userId, uint amount)
        {
            await _soraTransactor.DoInTransactionAsync(async context =>
            {
                var user = await context.Users.GetOrCreateUserNoSaveAsync(userId);
                if (user == null) return;

                user.Coins += amount;

                await context.SaveChangesAsync();
            }).ConfigureAwait(false);
        }

        public async Task<uint> GetCoins(ulong userId)
        {
            return await _soraTransactor.ReadUncommittedAsync<uint>(async context =>
            {
                // var user = await context.Users.FindAsync(userId);
                // return user?.Coins ?? (uint) 0;
                
                // This actually produces a better query where we do not fetch all the user data just for the
                // coin amount but actually just query for the coins
                return context.Users.Where(u => u.Id == userId).Select(u => u.Coins).FirstOrDefault();
            }).ConfigureAwait(false);
        }
    }
}