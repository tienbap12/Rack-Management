using Microsoft.EntityFrameworkCore;
using Rack.Domain.Commons.Abstractions;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Interfaces;
using Rack.MainInfrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rack.MainInfrastructure.Repositories
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : Entity
    {
        private readonly DbSet<TEntity> _dbSet;

        public GenericRepository(RackManagementContext context)
        {
            _dbSet = context.Set<TEntity>();
        }

        public IQueryable<TEntity> Query => _dbSet;

        public IQueryable<TEntity> BuildQuery => _dbSet.AsQueryable();

        public async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
        }

        public async Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _dbSet.ToListAsync(cancellationToken);
        }

        public async Task CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddAsync(entity, cancellationToken);
        }

        public async Task InsertRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
        {
            await _dbSet.AddRangeAsync(entities, cancellationToken);
        }

        // Trong GenericRepository.cs
        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _dbSet.FindAsync(new object[] { id }, cancellationToken);
            if (entity == null)
                // Cân nhắc trả về false hoặc null thay vì throw exception ở đây
                // để Handler có thể trả về NotFound Response.
                throw new ArgumentException($"Entity with id {id} not found.");

            // Chỉ cần gọi Remove. Hook trong SaveChanges sẽ xử lý ISoftDelete.
            _dbSet.Remove(entity);
        }
    }
}