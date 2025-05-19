using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Rack.API.Middleware
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UsePerformanceMonitoring(
            this IApplicationBuilder app,
            int slowRequestThresholdMs = 500)
        {
            return app.UseMiddleware<PerformanceMonitoringMiddleware>(slowRequestThresholdMs);
        }

        public static IServiceCollection AddRackOptimizations(this IServiceCollection services)
        {
            // Add performance related services here
            return services;
        }
    }
}