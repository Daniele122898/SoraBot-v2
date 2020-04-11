using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace SoraBot.Data
{
    /// <summary>
    /// Abstract implementation of the DB transactor that lacks the CreateContext function.
    /// </summary>
    /// <typeparam name="TContext">Type of context a concrete class will create and use</typeparam>
    public abstract class TransactorBase<TContext> : ITransactor<TContext> where TContext : DbContext
    {
        protected readonly ILogger<TransactorBase<TContext>> Logger;

        protected TransactorBase(ILogger<TransactorBase<TContext>> logger)
        {
            Logger = logger;
        }
        
        /// <summary>
        /// This must be implemented by whoever inherits this class to specify what kind of DB context
        /// shall be used
        /// </summary>
        /// <returns>The created DB context</returns>
        protected abstract TContext CreateContext();

        /// <inheritdoc />
        public T Do<T>(Func<TContext, T> action)
        {
            using var context = this.CreateContext();
            return action(context);
        }

        /// <inheritdoc />
        public async Task<T> DoAsync<T>(Func<TContext, Task<T>> task)
        {
            await using var context = this.CreateContext();
            return await task(context).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public void DoInTransaction(Action<TContext> action)
        {
            using var context = this.CreateContext();
            using var transaction = context.Database.BeginTransaction();
            try
            {
                action(context);
                // Transaction will auto-rollback when disposed if any commands fails.
                transaction.Commit();
            }
            catch (Exception e)
            {
                this.Logger.LogError(e, "Error in Transaction");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task DoInTransactionAsync(Func<TContext, Task> task)
        {
            await using var context = this.CreateContext();
            await using var transaction = await context.Database.BeginTransactionAsync().ConfigureAwait(false);
            try
            {
                await task(context).ConfigureAwait(false);
                // Transaction will auto-rollback when disposed if any commands fail
                await transaction.CommitAsync().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                this.Logger.LogError(e, "Error in async Transaction");
                throw;
            }
        }

        /// <inheritdoc />
        public async Task<T> ReadUncommittedAsync<T>(Func<TContext, Task<T>> task)
        {
            await using var context = this.CreateContext();
            await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.ReadUncommitted)
                .ConfigureAwait(false);
            return await task(context).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public T ReadUncommitted<T>(Func<TContext, T> action)
        {
            using var context = this.CreateContext();
            using var transaction = context.Database.BeginTransaction(IsolationLevel.ReadUncommitted);
            return action(context);
        }
    }
}