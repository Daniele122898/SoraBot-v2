using System;
using System.Threading.Tasks;
using ArgonautCore.Maybe;

namespace SoraBot.Data
{
    /// <summary>
    /// Interface used to abstract the DB context to allow for normal queries
    /// but also the use of atomic transactions
    /// </summary>
    /// <typeparam name="TContext">DB Context</typeparam>
    public interface ITransactor<TContext>
    {
        /// <summary>
        /// Carries out an operation and returns the result.
        /// This should be used for queries and updates
        /// </summary>
        /// <param name="action">The operation to be carried out</param>
        /// <typeparam name="T">Type of entity</typeparam>
        /// <returns></returns>
        T Do<T>(Func<TContext, T> action);

        /// <summary>
        /// Carries out an operation and returns the result asynchronously.
        /// This should be used for asynchronous queries and updates
        /// </summary>
        /// <param name="task">The operation to be carried out</param>
        /// <typeparam name="T">Type of entity</typeparam>
        /// <returns></returns>
        Task<T> DoAsync<T>(Func<TContext, Task<T>> task);

        /// <summary>
        /// Carries out an operation in a transaction.
        /// This should be used for update operations that are supposed to be atomic.
        /// </summary>
        /// <param name="action">The operation to be carried out</param>
        void DoInTransaction(Action<TContext> action);

        /// <summary>
        /// Carries out an operation in a transaction asynchronously.
        /// This should be used for update operations that are supposed to be atomic and async.
        /// </summary>
        /// <param name="task">The operation to be carried out</param>
        /// <returns></returns>
        Task DoInTransactionAsync(Func<TContext, Task> task);
        
        /// <summary>
        /// Carries out an operation in a transaction asynchronously.
        /// This should be used for update operations that are supposed to be atomic and async.
        /// This will NOT throw an exception on failure but simply return false.
        /// Use this for NON important transactions that might fail
        /// </summary>
        /// <param name="task">The operation to be carried out</param>
        /// <returns></returns>
        Task<bool> TryDoInTransactionAsync(Func<TContext, Task<bool>> task);

        /// <summary>
        /// Carries out an operation in a transaction asynchronously
        /// But also returns the result if successful.
        /// </summary>
        /// <param name="task"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<Maybe<T>> DoInTransactionAndGetAsync<T>(Func<TContext, Task<Maybe<T>>> task);

        /// <summary>
        /// This makes it possible to read uncommitted and dirty data from the DB.
        /// This poses a relaxation on the locks on the DB and makes stuff faster
        /// and less prone to deadlocks.
        /// ONLY USE FOR READING DATA
        /// </summary>
        /// <param name="task">The async action</param>
        /// <typeparam name="T">The type returned</typeparam>
        /// <returns></returns>
        Task<T> ReadUncommittedAsync<T>(Func<TContext, Task<T>> task);
        
        /// <summary>
        /// This makes it possible to read uncommitted and dirty data from the DB.
        /// This poses a relaxation on the locks on the DB and makes stuff faster
        /// and less prone to deadlocks.
        /// ONLY USE FOR READING DATA
        /// </summary>
        /// <param name="action">The action</param>
        /// <typeparam name="T">The type returned</typeparam>
        /// <returns></returns>
        T ReadUncommitted<T>(Func<TContext, T> action);
    }
}