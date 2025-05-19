using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Rack.API.Middleware
{
    public class PerformanceMonitoringMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<PerformanceMonitoringMiddleware> _logger;
        private readonly int _slowRequestThresholdMs;

        public PerformanceMonitoringMiddleware(
            RequestDelegate next,
            ILogger<PerformanceMonitoringMiddleware> logger,
            int slowRequestThresholdMs = 500)
        {
            _next = next;
            _logger = logger;
            _slowRequestThresholdMs = slowRequestThresholdMs;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();
                var elapsedMs = stopwatch.ElapsedMilliseconds;

                // Log all requests for monitoring
                var logLevel = elapsedMs > _slowRequestThresholdMs
                    ? LogLevel.Warning
                    : LogLevel.Debug;

                if (logLevel == LogLevel.Warning)
                {
                    _logger.Log(
                        logLevel,
                        "SLOW REQUEST: {Method} {Path} took {ElapsedMs}ms with status {StatusCode}",
                        context.Request.Method,
                        context.Request.Path,
                        elapsedMs,
                        context.Response.StatusCode);
                }
                else
                {
                    _logger.Log(
                        logLevel,
                        "REQUEST: {Method} {Path} took {ElapsedMs}ms with status {StatusCode}",
                        context.Request.Method,
                        context.Request.Path,
                        elapsedMs,
                        context.Response.StatusCode);
                }

                // Add timing header to response only if the response hasn't started
                if (!context.Response.HasStarted)
                {
                    context.Response.Headers["X-Request-Execution-Time"] = elapsedMs.ToString();
                }
            }
        }
    }
}