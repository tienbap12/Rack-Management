using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Rack.MainInfrastructure.Data;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rack.MainInfrastructure.Services
{
    public class DatabaseHealthCheck : IHealthCheck
    {
        private readonly RackManagementContext _dbContext;
        private readonly ILogger<DatabaseHealthCheck> _logger;

        public DatabaseHealthCheck(RackManagementContext dbContext, ILogger<DatabaseHealthCheck> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                // Simple check - can we connect to the database
                var canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);

                if (canConnect)
                {
                    _logger.LogInformation("Database health check passed at {Time}", DateTime.UtcNow);
                    return HealthCheckResult.Healthy("Database connection is healthy");
                }
                else
                {
                    _logger.LogWarning("Database health check failed at {Time}", DateTime.UtcNow);
                    return HealthCheckResult.Unhealthy("Cannot connect to database");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database health check error at {Time}", DateTime.UtcNow);
                return HealthCheckResult.Unhealthy("Database health check failed with exception", ex);
            }
        }
    }
}