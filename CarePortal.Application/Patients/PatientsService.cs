using CarePortal.Application.Abstractions.Persistence;
using CarePortal.Domain.Patients;

namespace CarePortal.Application.Patients;

public sealed class PatientsService
{
    private readonly IPatientRepository _patients;
    private readonly IUnitOfWork _unitOfWork;

    public PatientsService(IPatientRepository patients, IUnitOfWork unitOfWork)
    {
        _patients = patients;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<PatientResponse>> ListAsync(CancellationToken cancellationToken = default)
    {
        var patients = await _patients.ListAsync(cancellationToken);
        return patients.Select(ToResponse).ToList();
    }

    public async Task<PatientResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var patient = await _patients.GetByIdAsync(id, cancellationToken);
        return patient is null ? null : ToResponse(patient);
    }

    public async Task<PatientResponse> CreateAsync(CreatePatientRequest request, CancellationToken cancellationToken = default)
    {
        var patient = Patient.Create(request.FirstName, request.LastName, request.Email, request.DateOfBirth);

        await _patients.AddAsync(patient, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ToResponse(patient);
    }

    private static PatientResponse ToResponse(Patient patient) => new(
        patient.Id,
        patient.FirstName,
        patient.LastName,
        patient.Email,
        patient.DateOfBirth,
        patient.CreatedAtUtc);
}
