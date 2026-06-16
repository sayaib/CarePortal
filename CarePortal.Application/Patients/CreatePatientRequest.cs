namespace CarePortal.Application.Patients;

public sealed record CreatePatientRequest(string FirstName, string LastName, string Email, DateOnly DateOfBirth);
