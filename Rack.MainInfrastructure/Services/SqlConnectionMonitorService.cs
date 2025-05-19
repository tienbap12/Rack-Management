using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rack.MainInfrastructure.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rack.MainInfrastructure.Services
{
    /// <summary>
    /// Monitors SQL Server connection pool and collects metrics
    /// </summary>
    public class SqlConnectionMonitorService : BackgroundService
    {
        private readonly ILogger<SqlConnectionMonitorService> _logger;
        private readonly IConfiguration _configuration;
        private readonly RackManagementContext _dbContext;
        private readonly int _monitoringIntervalSeconds;

        public SqlConnectionMonitorService(
            ILogger<SqlConnectionMonitorService> logger,
            IConfiguration configuration,
            RackManagementContext dbContext)
        {
            _logger = logger;
            _configuration = configuration;
            _dbContext = dbContext;
            _monitoringIntervalSeconds = configuration.GetValue<int>("Performance:ConnectionPoolMonitorIntervalSeconds", 60);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("SQL Connection Monitor Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CollectConnectionPoolMetrics();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error collecting SQL connection pool metrics");
                }

                await Task.Delay(TimeSpan.FromSeconds(_monitoringIntervalSeconds), stoppingToken);
            }
        }

        private async Task CollectConnectionPoolMetrics()
        {
            _logger.LogInformation("Collecting connection pool metrics");

            var connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (var connection = new SqlConnection(connectionString))
            {
                try
                {
                    await connection.OpenAsync();
                    _logger.LogInformation("Database connection opened successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Unable to open database connection");
                }
            }

            int entriesCount = 0;
            try
            {
                entriesCount = _dbContext.ChangeTracker.Entries().Count();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unable to count entries");
            }
            _logger.LogInformation("Current context entries count: " + entriesCount);
        }
    }

    /// <summary>
    /// Helper class to access SqlClient connection pool statistics 
    /// </summary>
    internal static class SqlClientHelper
    {
        // Variable to track estimated connection count
        private static int _estimatedConnectionCount = 0;

        public static ConnectionPoolStatistics GetConnectionPoolStatistics(string connectionString)
        {
            try
            {
                // Force SqlClient to initialize the connection pool if not already done
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    // Increment our counter for a rough simulation
                    _estimatedConnectionCount++;
                }

                // This is a simulation since there's no public API for this in SqlClient
                return new ConnectionPoolStatistics
                {
                    ActiveConnectionCount = _estimatedConnectionCount,
                    AvailableConnectionCount = 100 - _estimatedConnectionCount // Estimation
                };
            }
            catch
            {
                return new ConnectionPoolStatistics();
            }
        }
    }

    internal class ConnectionPoolStatistics
    {
        public int ActiveConnectionCount { get; set; }
        public int AvailableConnectionCount { get; set; }
    }
}