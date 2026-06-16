using CarePortal.Domain.Patients;

namespace CarePortal.Application.Abstractions.Persistence;

public interface IPatientRepository
{
    Task<IReadOnlyList<Patient>> ListAsync(CancellationToken cancellationToken = default);
    Task<Patient?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(Patient patient, CancellationToken cancellationToken = default);
}
