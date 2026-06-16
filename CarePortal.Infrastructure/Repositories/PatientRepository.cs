using CarePortal.Application.Abstractions.Persistence;
using CarePortal.Domain.Patients;
using CarePortal.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarePortal.Infrastructure.Repositories;

public sealed class PatientRepository : IPatientRepository
{
    private readonly CarePortalDbContext _dbContext;

    public PatientRepository(CarePortalDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Patient>> ListAsync(CancellationToken cancellationToken = default) =>
        await _dbContext.Patients
            .AsNoTracking()
            .OrderBy(patient => patient.LastName)
            .ThenBy(patient => patient.FirstName)
            .ToListAsync(cancellationToken);

    public Task<Patient?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        _dbContext.Patients.FirstOrDefaultAsync(patient => patient.Id == id, cancellationToken);

    public async Task AddAsync(Patient patient, CancellationToken cancellationToken = default) =>
        await _dbContext.Patients.AddAsync(patient, cancellationToken);
}
