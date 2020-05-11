using System;
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

        public async Task<bool> TryTakeAmount(ulong userId, uint amount)
        {
            return await _soraTransactor.TryDoInTransactionAsync(async context =>
            {
                var user = await context.Users.FindAsync(userId).ConfigureAwait(false);
                if (user.Coins < amount) return false;
                user.Coins -= amount;
                await context.SaveChangesAsync().ConfigureAwait(false);
                return true;
            }).ConfigureAwait(false);
        }

        public async Task<bool> DoDaily(ulong userId, uint dailyAmount)
        {
            return await _soraTransactor.TryDoInTransactionAsync(async context =>
            {
                var user = await context.Users.FindAsync(userId).ConfigureAwait(false);
                user.LastDaily = DateTime.UtcNow;
                user.Coins += dailyAmount;
                await context.SaveChangesAsync().ConfigureAwait(false);
                return true;
            }).ConfigureAwait(false);
        }

        public async Task<bool> TryMakeTransfer(ulong userId, ulong receiverId, uint amount)
        {
            return await _soraTransactor.TryDoInTransactionAsync(async context =>
            {
                var user = await context.Users.FindAsync(userId).ConfigureAwait(false);
                var receiver = await context.Users.FindAsync(receiverId).ConfigureAwait(false);
                if (user == null || receiver == null) return false;
                if (user.Coins < amount) return false;
                user.Coins -= amount;
                receiver.Coins += amount;
                await context.SaveChangesAsync().ConfigureAwait(false);
                return true;
            }).ConfigureAwait(false);
        }

        public uint GetCoins(ulong userId) =>
            _soraTransactor.ReadUncommitted<uint>(context =>
                context.Users.Where(u => u.Id == userId).Select(u => u.Coins).FirstOrDefault());
    }
}