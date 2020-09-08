using System.Threading.Tasks;
using ArgonautCore.Lw;
using SoraBot.Data.Models.SoraDb;
using SoraBot.Data.Repositories.Interfaces;

namespace SoraBot.Data.Repositories
{
    public class AfkRepository : IAfkRepository
    {
        private readonly ITransactor<SoraContext> _soraTransactor;
        
        public AfkRepository(ITransactor<SoraContext> soraTransactor)
        {
            _soraTransactor = soraTransactor;
        }

        public async Task<Option<Afk>> GetUserAfk(ulong userId)
            => await _soraTransactor.DoAsync(async context =>
            {
                var afk = await context.Afks.FindAsync(userId).ConfigureAwait(false);
                if (afk == null)
                    return Option.None<Afk>();

                return afk;
            }).ConfigureAwait(false);

        public async Task RemoveUserAfk(ulong userId)
            => await _soraTransactor.DoInTransactionAsync(async context =>
            {
                var afk = await context.Afks.FindAsync(userId).ConfigureAwait(false);
                if (afk == null) return;

                context.Afks.Remove(afk);
                await context.SaveChangesAsync().ConfigureAwait(false);
            }).ConfigureAwait(false);

        public async Task SetUserAfk(ulong userId, string message)
            => await _soraTransactor.DoInTransactionAsync(async context =>
            {
                var afk = await context.Afks.FindAsync(userId).ConfigureAwait(false);
                if (afk == null)
                {
                    afk = new Afk()
                    {
                        UserId = userId,
                        Message = message
                    };
                    context.Afks.Add(afk);
                }
                else
                    afk.Message = message;

                await context.SaveChangesAsync().ConfigureAwait(false);
            }).ConfigureAwait(false);
    }
}