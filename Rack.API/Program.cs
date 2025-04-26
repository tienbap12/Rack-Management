using System.Text.Json;
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
builder.Services.AddControllers().AddJsonOptions(options =>
{
    //options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false));
});

builder.Services.AddCorsConfig();
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

app.UseRouting();
app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/chat-hub");
app.Run();