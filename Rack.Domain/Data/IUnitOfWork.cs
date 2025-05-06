using Rack.Domain.Commons.Primitives;
using Rack.Domain.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rack.Domain.Data
{
    public interface IUnitOfWork : IDisposable
    {
        Task<int> SaveChangesAsync(CancellationToken cancellation = default);

        Task BeginTransactionAsync(CancellationToken cancellationToken = default);

        Task CommitAsync(CancellationToken cancellationToken = default);

        Task RollBackAsync(CancellationToken cancellationToken = default);

        IGenericRepository<TEntity> GetRepository<TEntity>() where TEntity : Entity;

        /// <summary>
        /// Executes a function within a transaction
        /// </summary>
        /// <typeparam name="TResult">The type of the result</typeparam>
        /// <param name="action">The function to execute</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The result of the function</returns>
        Task<TResult> ExecuteInTransactionAsync<TResult>(
            Func<Task<TResult>> action,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if there is an active transaction
        /// </summary>
        bool HasActiveTransaction { get; }
    }
}