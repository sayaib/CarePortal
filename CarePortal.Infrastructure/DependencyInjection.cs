using CarePortal.Application.Abstractions.Persistence;
using CarePortal.Infrastructure.Persistence;
using CarePortal.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CarePortal.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

        services.AddDbContext<CarePortalDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsAssembly(typeof(CarePortalDbContext).Assembly.FullName)));

        services.AddScoped<IPatientRepository, PatientRepository>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<CarePortalDbContext>());

        return services;
    }
}
