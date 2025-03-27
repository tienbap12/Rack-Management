using Rack.Domain.Commons.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rack.Domain.Interfaces;

public interface IGenericRepository<TEntity> where TEntity : Entity
{
    Task<TEntity> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken);
    Task CreateAsync(TEntity entity, CancellationToken cancellationToken);
    Task InsertRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
    IQueryable<TEntity> BuildQuery { get; }
}
