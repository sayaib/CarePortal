namespace CarePortal.Application.Patients;

public sealed record PatientResponse(Guid Id, string FirstName, string LastName, string Email, DateOnly DateOfBirth, DateTimeOffset CreatedAtUtc);
