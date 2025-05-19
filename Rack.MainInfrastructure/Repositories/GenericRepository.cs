using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Rack.Domain.Commons.Abstractions;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Interfaces;
using Rack.MainInfrastructure.Data;
using Rack.MainInfrastructure.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

namespace Rack.MainInfrastructure.Repositories
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : Entity
    {
        private readonly DbSet<TEntity> _dbSet;
        private readonly RackManagementContext _context;
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(10);
        private string CacheKeyPrefix => $"entity_{typeof(TEntity).Name}_";

        public GenericRepository(RackManagementContext context, IMemoryCache cache)
        {
            _context = context;
            _dbSet = context.Set<TEntity>();
            _cache = cache;
        }

        public IQueryable<TEntity> Query => _dbSet;

        public IQueryable<TEntity> BuildQuery => _dbSet.AsQueryable();

        public async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var cacheKey = $"{CacheKeyPrefix}{id}";
            if (!_cache.TryGetValue(cacheKey, out TEntity? entity))
            {
                entity = await _dbSet.FindAsync(new object[] { id }, cancellationToken);
                if (entity != null)
                {
                    var cacheOptions = new MemoryCacheEntryOptions()
                        .SetAbsoluteExpiration(_cacheExpiration)
                        .SetPriority(CacheItemPriority.High)
                        .SetSize(1);
                    _cache.Set(cacheKey, entity, cacheOptions);
                }
            }
            return entity;
        }

        public async Task<TEntity?> GetByIdWithTrackingAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AsTracking()
                .Where(e => e.Id == id)
                .FirstOrDefaultOptimizedAsync(cancellationToken);
        }

        public async Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var cacheKey = $"{CacheKeyPrefix}all";
            if (!_cache.TryGetValue(cacheKey, out List<TEntity>? entities))
            {
                // For potentially large sets, use batched loading
                entities = await _dbSet.AsNoTracking()
                    .ToListBatchedAsync(batchSize: 1000, cancellationToken);

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(_cacheExpiration)
                    .SetPriority(CacheItemPriority.Normal)
                    .SetSize(entities?.Count ?? 0);
                _cache.Set(cacheKey, entities, cacheOptions);
            }
            return entities ?? new List<TEntity>();
        }

        public async Task<List<TEntity>> GetAllWithTrackingAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet.ToListBatchedAsync(batchSize: 1000, cancellationToken);
        }

        public async Task<int> CountAllAsync(CancellationToken cancellationToken = default)
        {
            var cacheKey = $"{CacheKeyPrefix}count";
            if (!_cache.TryGetValue(cacheKey, out int count))
            {
                count = await _dbSet.CountBatchedAsync(cancellationToken: cancellationToken);

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(_cacheExpiration)
                    .SetPriority(CacheItemPriority.Normal)
                    .SetSize(1);

                _cache.Set(cacheKey, count, cacheOptions);
            }

            return count;
        }

        public async Task CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddAsync(entity, cancellationToken);
            InvalidateCache();
        }

        public async Task InsertRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddRangeAsync(entities, cancellationToken);
            InvalidateCache();
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _dbSet.FindAsync(new object[] { id }, cancellationToken);
            if (entity == null)
                throw new ArgumentException($"Entity with id {id} not found.");

            _dbSet.Remove(entity);
            InvalidateEntityCache(id);
        }

        public async Task DeleteRangeAsync(IQueryable<TEntity> query, CancellationToken cancellationToken)
        {
            // Load in batches to avoid memory issues with large sets
            var entities = await query.ToListBatchedAsync(cancellationToken: cancellationToken);
            _dbSet.RemoveRange(entities);
            InvalidateCache();
        }

        public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _dbSet.AnyAsync(predicate, cancellationToken);
        }

        public async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _dbSet.Where(predicate).CountBatchedAsync(cancellationToken: cancellationToken);
        }

        public async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            // Optimize query by using FirstOrDefaultAsync directly without materializing the query
            return await _dbSet.AsNoTracking()
                .Where(predicate)
                .FirstOrDefaultOptimizedAsync(cancellationToken);
        }

        public async Task<List<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _dbSet.Where(predicate)
                .AsNoTracking()
                .ToListBatchedAsync(batchSize: 1000, cancellationToken);
        }

        public async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            var entry = _context.Entry(entity);
            if (entry.State == EntityState.Detached)
            {
                _dbSet.Attach(entity);
                entry = _context.Entry(entity);
            }
            if (entry.State != EntityState.Modified)
            {
                entry.State = EntityState.Modified;
            }
            await _context.SaveChangesAsync(cancellationToken);
            InvalidateEntityCache(entity.Id);
        }

        public async Task UpdateRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            _dbSet.UpdateRange(entities);
            await _context.SaveChangesAsync(cancellationToken);
            InvalidateCache();
        }

        // Helper methods for cache invalidation
        private void InvalidateCache()
        {
            var allCacheKey = $"{CacheKeyPrefix}all";
            var countCacheKey = $"{CacheKeyPrefix}count";
            _cache.Remove(allCacheKey);
            _cache.Remove(countCacheKey);
            if (typeof(TEntity).Name == "Device")
            {
                _cache.Remove("devices_all");
            }
        }

        private void InvalidateEntityCache(Guid id)
        {
            var cacheKey = $"{CacheKeyPrefix}{id}";
            _cache.Remove(cacheKey);
            InvalidateCache(); // Also invalidate the collection cache
        }
    }
}