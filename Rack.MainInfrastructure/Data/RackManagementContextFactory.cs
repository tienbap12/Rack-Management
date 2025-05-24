using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Rack.MainInfrastructure.Data;

public class RackManagementContextFactory()
    : IDesignTimeDbContextFactory<RackManagementContext>
{
    public RackManagementContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<RackManagementContext>();
        optionsBuilder.UseSqlServer("Server=103.9.156.221,1433;Database=RackManagementDB;User Id=admin;Password=Admin@@6789@@;TrustServerCertificate=True;");

        return new RackManagementContext(optionsBuilder.Options);
    }
}