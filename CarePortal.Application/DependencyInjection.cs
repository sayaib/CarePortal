using CarePortal.Application.Patients;
using Microsoft.Extensions.DependencyInjection;

namespace CarePortal.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<PatientsService>();
        return services;
    }
}

