using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rack.Application.Commons.Abstractions;
using Rack.Domain.Commons.Abstractions;
using Rack.Domain.Data;
using Rack.MainInfrastructure.Common.Authentication;
using Rack.MainInfrastructure.Common.Cryptography;
using Rack.MainInfrastructure.Data;

namespace Rack.MainInfrastructure{
    public static class DependencyInjection{
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    optionsBuilder => optionsBuilder
                        .MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

            //DI UnitOfWork
            services.AddScoped<IUnitOfWork, ApplicationDbContext>();

            //DI Authentication
            services.AddScoped<IJwtProvider, JwtProvider>();
            services.AddScoped<IPasswordHashChecker, PasswordHasher>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();

            return services;
        }
    }
}