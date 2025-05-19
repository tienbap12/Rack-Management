using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Rack.API.Extensions;
using Rack.API.Middleware;
using Rack.API.Options;
using Rack.API.WebSocket;
using Rack.Application;
using Rack.MainInfrastructure;
using Rack.MainInfrastructure.Common.Authentication;
using Rack.MainInfrastructure.Services;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);

// Add performance optimizations (includes response compression)
builder.Services.AddPerformanceOptimizations(builder.Configuration);

// Add Rack-specific optimizations
builder.Services.AddRackOptimizations();

// Health checks are already registered in DependencyInjection.cs
// Do not add duplicate health checks here

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false));
});

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication(builder.Configuration);
builder.Services.ConfigService(builder.Configuration);

builder.Services.AddCorsConfig();
builder.Services.AddSignalR();
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));
var app = builder.Build();

// Add performance monitoring middleware
app.UsePerformanceMonitoring(
    slowRequestThresholdMs: builder.Configuration.GetValue<int>("Performance:SlowRequestThresholdMs", 500));

// Enable Response Compression Middleware
app.UseResponseCompression();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
    app.UseDeveloperExceptionPage();
else
    app.UseHsts();

var swaggerOptions = app.Configuration.GetSection(nameof(SwaggerOptions)).Get<SwaggerOptions>();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint(swaggerOptions.UIEndpoint, swaggerOptions.Description ?? "My API V1");
});

// Add health check endpoint
app.UseHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            Status = report.Status.ToString(),
            HealthChecks = report.Entries.Select(e => new
            {
                Component = e.Key,
                Status = e.Value.Status.ToString(),
                Description = e.Value.Description,
                Duration = e.Value.Duration
            }),
            TotalDuration = report.TotalDuration
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
});

app.UseRouting();
app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/chat-hub");
app.Run();