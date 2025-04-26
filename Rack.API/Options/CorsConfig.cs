using Microsoft.Extensions.DependencyInjection;

namespace Rack.API.Options;

public static class CorsConfig
{
    public static void AddCorsConfig(this IServiceCollection service)
    {
        service.AddCors(options =>
        {
            options.AddPolicy(name: "CorsPolicy",
                              policy =>
                              {
                                  policy.WithOrigins("http://localhost:3000")
                                        .AllowAnyHeader()
                                        .AllowAnyMethod();
                              });
        });
    }
}