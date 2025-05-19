using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;
using Rack.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rack.MainInfrastructure.Services
{
    public class CacheRefreshService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CacheRefreshService> _logger;
        private readonly int _refreshIntervalMinutes;
        private readonly Dictionary<string, TimeSpan> _refreshTimes = new();

        public CacheRefreshService(
            IServiceProvider serviceProvider,
            ILogger<CacheRefreshService> logger,
            IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;

            // Get refresh interval from configuration, default to 15 minutes
            _refreshIntervalMinutes = configuration.GetSection("Caching:RefreshInterval").Get<int?>() ?? 15;

            // Configure refresh times for different entity types
            var entitySection = configuration.GetSection("Caching:EntityCache");

            _refreshTimes["DeviceRack"] = TimeSpan.FromSeconds(entitySection.GetValue<int>("DeviceExpiration", 900));
            _refreshTimes["DataCenter"] = TimeSpan.FromSeconds(entitySection.GetValue<int>("DataCenterExpiration", 1800));
            _refreshTimes["Customer"] = TimeSpan.FromSeconds(entitySection.GetValue<int>("CustomerExpiration", 1800));
            _refreshTimes["Device"] = TimeSpan.FromSeconds(entitySection.GetValue<int>("DeviceExpiration", 900));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Cache Refresh Service is starting");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await RefreshCommonCaches(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while refreshing caches");
                }

                // Wait for the next refresh interval
                await Task.Delay(TimeSpan.FromMinutes(_refreshIntervalMinutes), stoppingToken);
            }
        }

        private async Task RefreshCommonCaches(CancellationToken cancellationToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var cache = scope.ServiceProvider.GetRequiredService<IMemoryCache>();

                // Refresh device racks cache
                await RefreshEntityCache<DeviceRack>(unitOfWork, cache, "device_racks_all", cancellationToken);

                // Refresh data centers cache
                await RefreshEntityCache<DataCenter>(unitOfWork, cache, "data_centers_all", cancellationToken);

                // Refresh customers cache
                await RefreshEntityCache<Customer>(unitOfWork, cache, "customers_all", cancellationToken);

                // Refresh devices cache
                await RefreshEntityCache<Device>(unitOfWork, cache, "devices_all", cancellationToken);
            }
        }

        private async Task RefreshEntityCache<TEntity>(
            IUnitOfWork unitOfWork,
            IMemoryCache cache,
            string cacheKey,
            CancellationToken cancellationToken) where TEntity : Entity
        {
            _logger.LogInformation("Refreshing cache for {EntityType}", typeof(TEntity).Name);

            try
            {
                var repository = unitOfWork.GetRepository<TEntity>();
                var entities = await repository.GetAllAsync(cancellationToken);

                var entityName = typeof(TEntity).Name;
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(_refreshTimes.TryGetValue(entityName, out var time) ? time : TimeSpan.FromMinutes(30))
                    .SetPriority(CacheItemPriority.Normal)
                    .SetSize(entities.Count);

                cache.Set(cacheKey, entities, cacheOptions);
                _logger.LogInformation("Successfully refreshed cache for {EntityType} with {Count} items",
                    typeof(TEntity).Name, entities.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing cache for {EntityType}", typeof(TEntity).Name);
            }
        }
    }
}