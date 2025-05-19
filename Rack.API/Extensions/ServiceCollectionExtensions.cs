using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rack.MainInfrastructure.Data;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;

namespace Rack.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds various performance optimizations to the application
        /// </summary>
        public static IServiceCollection AddPerformanceOptimizations(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Add response compression
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<GzipCompressionProvider>();
                options.MimeTypes = GetCompressionMimeTypes();
            });

            services.Configure<GzipCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Fastest; // Balance between CPU usage and compression ratio
            });

            // Configure EF Core with performance optimizations
            services.AddDbContext<RackManagementContext>(
                (serviceProvider, options) =>
                {
                    // Get the connection string from configuration
                    var connectionString = configuration.GetConnectionString("DefaultConnection");
                    options.UseSqlServer(connectionString, sqlOptions =>
                    {
                        // Enable resilient connection retry
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null);
                    });

                    // Enable batch query support for better performance
                    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                });

            // Add memory cache with size limits
            services.AddMemoryCache(options =>
            {
                var sizeLimit = configuration.GetValue<long?>("Caching:MemoryCache:SizeLimit");
                if (sizeLimit.HasValue)
                {
                    options.SizeLimit = sizeLimit.Value;
                }
                options.CompactionPercentage = 0.2; // 20% reduction when limit is reached
            });

            return services;
        }

        private static IEnumerable<string> GetCompressionMimeTypes()
        {
            return new[]
            {
                "application/json",
                "application/vnd.api+json",
                "application/javascript",
                "application/xml",
                "text/css",
                "text/html",
                "text/json",
                "text/plain",
                "text/xml"
            };
        }
    }
}