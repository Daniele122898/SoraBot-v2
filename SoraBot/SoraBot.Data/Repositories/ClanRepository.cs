using SoraBot.Data.Repositories.Interfaces;

namespace SoraBot.Data.Repositories
{
    public class ClanRepository : IClanRepository
    {
        private readonly ITransactor<SoraContext> _soraTransactor;
        
        public ClanRepository(ITransactor<SoraContext> soraTransactor)
        {
            _soraTransactor = soraTransactor;
        }
    }
}