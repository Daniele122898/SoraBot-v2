using System;
using Microsoft.Extensions.Logging;

namespace SoraBot.Data
{
    public class SoraDbTransactor : TransactorBase<SoraContext>
    {
        private readonly Func<SoraContext> _contextFactory;

        public SoraDbTransactor(ILogger<TransactorBase<SoraContext>> logger, Func<SoraContext> contextFactory) : base(logger)
        {
            _contextFactory = contextFactory;
        }

        protected override SoraContext CreateContext()
        {
            return _contextFactory();
        }
    }
}