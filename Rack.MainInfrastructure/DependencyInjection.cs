using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rack.Application.Commons.Abstractions;
using Rack.Application.Commons.Interfaces;
using Rack.Domain.Commons.Abstractions;
using Rack.Domain.Data;
using Rack.Domain.Interfaces;
using Rack.MainInfrastructure.Common.Authentication;
using Rack.MainInfrastructure.Common.Cryptography;
using Rack.MainInfrastructure.Data;
using Rack.MainInfrastructure.Repositories;
using Rack.MainInfrastructure.Services;

namespace Rack.MainInfrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<RackManagementContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(RackManagementContext).Assembly.FullName)));
            services.AddHttpContextAccessor();
            services.AddMemoryCache();
            //DI UnitOfWork
            services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<RackManagementContext>());
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IUserContext, UserContext>();
            services.AddHostedService<TokenCleanupService>();
            //DI Authentication
            services.AddScoped<IJwtProvider, JwtProvider>();
            services.AddScoped<IPasswordHashChecker, PasswordHasher>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<IMinIOService, MinioService>();
            services.AddHttpClient<IZabbixService, ZabbixService>();
            return services;
        }
    }
}