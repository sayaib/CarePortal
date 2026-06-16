using CarePortal.Application.Abstractions.Persistence;
using CarePortal.Domain.Patients;
using Microsoft.EntityFrameworkCore;

namespace CarePortal.Infrastructure.Persistence;

public sealed class CarePortalDbContext : DbContext, IUnitOfWork
{
    public CarePortalDbContext(DbContextOptions<CarePortalDbContext> options)
        : base(options)
    {
    }

    public DbSet<Patient> Patients => Set<Patient>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CarePortalDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
