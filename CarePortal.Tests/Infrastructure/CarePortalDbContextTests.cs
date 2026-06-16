using CarePortal.Domain.Patients;
using CarePortal.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarePortal.Tests.Infrastructure;

public sealed class CarePortalDbContextTests
{
    [Fact]
    public async Task SaveChanges_PersistsPatient()
    {
        var options = new DbContextOptionsBuilder<CarePortalDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new CarePortalDbContext(options);
        var patient = Patient.Create("Grace", "Hopper", "grace@example.com", new DateOnly(1906, 12, 9));

        dbContext.Patients.Add(patient);
        await dbContext.SaveChangesAsync();

        var persisted = await dbContext.Patients.SingleAsync();
        Assert.Equal(patient.Id, persisted.Id);
    }
}
