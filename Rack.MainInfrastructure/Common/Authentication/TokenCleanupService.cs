using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rack.Domain.Data;
using Rack.Domain.Entities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rack.MainInfrastructure.Common.Authentication;

public class TokenCleanupService : BackgroundService
{
    private readonly IServiceProvider _services;

    public TokenCleanupService(IServiceProvider services)
    {
        _services = services;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _services.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var repo = unitOfWork.GetRepository<RefreshToken>();
            var query = repo.BuildQuery
            .Where(x => x.ExpiryDate < DateTime.UtcNow);
            await repo.DeleteRangeAsync(query, stoppingToken);
            await unitOfWork.SaveChangesAsync(stoppingToken);

            await Task.Delay(TimeSpan.FromHours(6), stoppingToken); // Dọn dẹp 6 tiếng/lần
        }
    }
}