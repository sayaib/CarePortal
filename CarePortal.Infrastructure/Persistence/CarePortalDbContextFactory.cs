using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace CarePortal.Infrastructure.Persistence;

public sealed class CarePortalDbContextFactory : IDesignTimeDbContextFactory<CarePortalDbContext>
{
    public CarePortalDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=careportal;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<CarePortalDbContext>()
            .UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsAssembly(typeof(CarePortalDbContext).Assembly.FullName))
            .Options;

        return new CarePortalDbContext(options);
    }
}
