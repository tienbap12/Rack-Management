using FluentValidation;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Rack.Application.Commons.Behaviors;
using Rack.Application.Commons.Services;
using System.Text;

namespace Rack.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddSingleton<IMapper, Mapper>();

        // Register application services
        services.AddScoped<CommandBatchService>();

        // Validators (FluentValidation)
        services.AddValidatorsFromAssembly(assembly);

        // MediatR & Pipeline Behaviors
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(assembly);

            // Add behaviors in proper order: Validation → UnitOfWork → ResponseWrapper
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
            config.AddOpenBehavior(typeof(UnitOfWorkBehavior<,>));
            config.AddOpenBehavior(typeof(ResponseWrapperBehavior<,>));
        });

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = configuration["JwtSettings:Issuer"],
                ValidAudience = configuration["JwtSettings:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["JwtSettings:SecurityKey"])
                    ),
                RoleClaimType = "Role",
            };
        });

        return services;
    }
}