using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rack.Domain.Commons.Abstractions;
using Rack.Application.Commons.Interfaces;
using Rack.Domain.Data;
using Rack.Domain.Interfaces;
using Rack.MainInfrastructure.Common.Authentication;
using Rack.MainInfrastructure.Common.Cryptography;
using Rack.MainInfrastructure.Data;
using Rack.MainInfrastructure.Repositories;
using Rack.MainInfrastructure.Services;
using System;

namespace Rack.MainInfrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Register DbContext with optimized options
            services.AddDbContext<RackManagementContext>(options =>
            {
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions =>
                    {
                        sqlOptions.MigrationsAssembly(typeof(RackManagementContext).Assembly.FullName);
                        // Enable connection resiliency
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null);
                        // Optimize batch operations
                        sqlOptions.CommandTimeout(60);
                    });

                // Enable batched SaveChanges for better performance
                options.EnableSensitiveDataLogging(false);

                // Optimize change tracking - only track entities where needed
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            });

            // Add memory caching
            services.AddMemoryCache(options =>
            {
                // Set size limits to prevent cache from consuming too much memory
                options.SizeLimit = 1024 * 1024 * 100; // 100MB limit
                options.CompactionPercentage = 0.2; // 20% reduction when limit is reached
            });

            services.AddHttpContextAccessor();

            // DI UnitOfWork
            services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<RackManagementContext>());
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IUserContext, UserContext>();

            // Register health check
            services.AddHealthChecks()
                .AddCheck<DatabaseHealthCheck>("database");

            // Register background services
            services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
            services.AddHostedService<BackgroundTaskService>();
            services.AddScoped<BackgroundTaskService>();

            // Register cache refresh service
            services.AddHostedService<CacheRefreshService>();

            // Register SQL connection monitor service
            if (configuration.GetValue<bool>("Performance:EnableConnectionPoolMonitoring", false))
            {
                services.AddHostedService<SqlConnectionMonitorService>();
            }

            // Register cleanup services
            services.AddHostedService<TokenCleanupService>();

            // DI Authentication
            services.AddScoped<IJwtProvider, JwtProvider>();
            services.AddScoped<IPasswordHashChecker, PasswordHasher>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IMinIOService, MinioService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();

            // Configure HttpClient with resiliency
            services.AddHttpClient<IZabbixService, ZabbixService>(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .SetHandlerLifetime(TimeSpan.FromMinutes(5)); // Optimize connection pooling

            return services;
        }
    }
}