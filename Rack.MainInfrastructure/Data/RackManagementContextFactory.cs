using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Rack.MainInfrastructure.Data;

public class RackManagementContextFactory(IHttpContextAccessor httpContextAccessor)
    : IDesignTimeDbContextFactory<RackManagementContext>
{
    public RackManagementContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<RackManagementContext>();
        optionsBuilder.UseSqlServer("Server=THANHTIEN-24695;Database=RackManagementDB;User Id=sa;Password=123;TrustServerCertificate=True;");

        return new RackManagementContext(optionsBuilder.Options, httpContextAccessor);
    }
}