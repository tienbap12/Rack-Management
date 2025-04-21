using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Rack.Application.Commons.Interfaces;
using Rack.Domain.Commons.Abstractions;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;
using Rack.Domain.Entities;
using Rack.Domain.Interfaces;
using Rack.MainInfrastructure.Repositories;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Rack.MainInfrastructure.Data
{
    public class RackManagementContext : DbContext, IUnitOfWork
    {
        private IDbContextTransaction? _transaction;
        private readonly IUserContext _userContext;
        private readonly ConcurrentDictionary<Type, object> _repositories = new();

        public RackManagementContext(
            DbContextOptions<RackManagementContext> options,
            IUserContext userContext)
            : base(options)
        {
            _userContext = userContext ??
                throw new ArgumentNullException(nameof(userContext));
        }

        public RackManagementContext(DbContextOptions<RackManagementContext> options)
        : base(options)
        {
        }

        // Expression-bodied DbSet properties for concise code
        public DbSet<DataCenter> DataCenters => Set<DataCenter>();

        public DbSet<DeviceRack> Racks => Set<DeviceRack>();
        public DbSet<Device> Devices => Set<Device>();
        public DbSet<ConfigurationItem> ConfigurationItems => Set<ConfigurationItem>();
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<ServerRental> ServerRentals => Set<ServerRental>();
        public DbSet<Account> Accounts => Set<Account>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<Role> Roles => Set<Role>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply configurations from the Entity assembly
            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(Entity).Assembly,
                type => type.Namespace == typeof(Entity).Namespace
            );

            ConfigureEntityRelations(modelBuilder);
            ConfigureQueryFilters(modelBuilder);
            //ConfigureSoftDeleteGlobalFilter(modelBuilder);
        }

        private void ConfigureEntityRelations(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DeviceRack>()
                .HasOne(r => r.DataCenter)
                .WithMany(dc => dc.Racks)
                .HasForeignKey(r => r.DataCenterID)
                .OnDelete(DeleteBehavior.ClientCascade);

            modelBuilder.Entity<Device>()
                .HasOne(d => d.ParentDevice)
                .WithMany(d => d.ChildDevices)
                .HasForeignKey(d => d.ParentDeviceID)
                .OnDelete(DeleteBehavior.ClientNoAction);

            modelBuilder.Entity<Device>()
                .HasOne(d => d.Rack)
                .WithMany(r => r.Devices)
                .HasForeignKey(d => d.RackID)
                .OnDelete(DeleteBehavior.ClientSetNull);
        }

        private void ConfigureQueryFilters(ModelBuilder modelBuilder)
        {
            // Advanced query filter configuration
            var softDeleteMethod = typeof(EF).GetMethod(nameof(EF.Property))!
                .MakeGenericMethod(typeof(bool));

            foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                .Where(e => typeof(ISoftDelete).IsAssignableFrom(e.ClrType)))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var isDeletedProperty = Expression.Call(
                    softDeleteMethod,
                    parameter,
                    Expression.Constant("IsDeleted")
                );
                var lambda = Expression.Lambda(
                    Expression.Equal(isDeletedProperty, Expression.Constant(false)),
                    parameter
                );

                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }

        private void UpdateAuditableEntities()
        {
            var utcNow = DateTime.UtcNow;
            var currentUser = _userContext.GetUsername();
            Console.WriteLine($"Current User: {currentUser}");
            foreach (var entry in ChangeTracker.Entries<IAuditInfo>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedOn = utcNow;
                        entry.Entity.CreatedBy = currentUser;
                        break;

                    case EntityState.Modified:
                        entry.Entity.LastModifiedOn = utcNow;
                        entry.Entity.LastModifiedBy = currentUser;
                        break;
                }
            }
        }

        private void HandleSoftDelete()
        {
            var utcNow = DateTime.UtcNow;
            var currentUser = _userContext.GetUsername();

           foreach (var entry in ChangeTracker.Entries<ISoftDelete>())
            {
                if (entry.State == EntityState.Deleted)
                {
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedOn = utcNow;
                    entry.Entity.DeletedBy = currentUser;
                }
            }
        }

        #region UnitOfWork Implementation

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            _transaction ??= await Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (_transaction != null)
                {
                    await _transaction.CommitAsync(cancellationToken);
                    await _transaction.DisposeAsync();
                }
            }
            finally
            {
                _transaction = null;
            }
        }

        public async Task RollBackAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync(cancellationToken);
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        #endregion UnitOfWork Implementation

        #region Extended Methods

        public void DetachAllEntities()
        {
            var changedEntries = ChangeTracker.Entries()
                .Where(e => e.State != EntityState.Detached)
                .ToList();

            foreach (var entry in changedEntries)
            {
                entry.State = EntityState.Detached;
            }
        }

        #endregion Extended Methods

        public IGenericRepository<TEntity> GetRepository<TEntity>() where TEntity : Entity
        {
            return new GenericRepository<TEntity>(this);
        }

        async Task<int> IUnitOfWork.SaveChangesAsync(CancellationToken cancellation)
        {
            UpdateAuditableEntities();
            HandleSoftDelete();
            return await base.SaveChangesAsync(cancellation);
        }
    }
}