using CarePortal.Application.Abstractions.Billing;
using CarePortal.Infrastructure.Billing;
using CarePortal.Infrastructure.Persistence;
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
            options.UseSqlite(connectionString));

        services.AddScoped<IBillingPaymentAllocator, EfCoreBillingPaymentAllocator>();

        return services;
    }
}
