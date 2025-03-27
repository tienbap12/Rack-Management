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
    }
}