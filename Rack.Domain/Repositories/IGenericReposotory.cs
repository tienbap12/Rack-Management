using Rack.Domain.Commons.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Rack.Domain.Interfaces;

public interface IGenericRepository<TEntity> where TEntity : Entity
{
    // Query properties
    IQueryable<TEntity> Query { get; }
    IQueryable<TEntity> BuildQuery { get; }

    // Get methods
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TEntity?> GetByIdWithTrackingAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<TEntity>> GetAllWithTrackingAsync(CancellationToken cancellationToken = default);
    Task<int> CountAllAsync(CancellationToken cancellationToken = default);

    // Create methods
    Task CreateAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task InsertRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    // Update methods
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    void SetEntityState(TEntity entity, EntityState state);

    // Delete methods
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeleteRangeAsync(IQueryable<TEntity> query, CancellationToken cancellationToken = default);

    // Query methods
    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<List<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
}