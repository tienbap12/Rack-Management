using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rack.API.Options;
using Rack.API.WebSocket;
using Rack.Application;
using Rack.MainInfrastructure;
using Rack.MainInfrastructure.Common.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication(builder.Configuration);
builder.Services.ConfigService(builder.Configuration);
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
});

builder.Services.AddSignalR();
builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));
var app = builder.Build();

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

// Apply CORS policy
app.UseCors("CorsPolicy");

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    _ = endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});
app.MapHub<ChatHub>("/chat-hub");
app.Run();