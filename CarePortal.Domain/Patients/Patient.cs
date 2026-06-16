using CarePortal.Domain.Common;

namespace CarePortal.Domain.Patients;

public sealed class Patient : Entity
{
    private Patient()
    {
        FirstName = string.Empty;
        LastName = string.Empty;
        Email = string.Empty;
    }

    private Patient(string firstName, string lastName, string email, DateOnly dateOfBirth)
    {
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Email = email.Trim().ToLowerInvariant();
        DateOfBirth = dateOfBirth;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Email { get; private set; }
    public DateOnly DateOfBirth { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }

    public static Patient Create(string firstName, string lastName, string email, DateOnly dateOfBirth)
    {
        if (string.IsNullOrWhiteSpace(firstName)) throw new ArgumentException("First name is required.", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName)) throw new ArgumentException("Last name is required.", nameof(lastName));
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required.", nameof(email));
        if (dateOfBirth > DateOnly.FromDateTime(DateTime.UtcNow)) throw new ArgumentException("Date of birth cannot be in the future.", nameof(dateOfBirth));

        return new Patient(firstName, lastName, email, dateOfBirth);
    }

    public void UpdateContactEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email is required.", nameof(email));
        Email = email.Trim().ToLowerInvariant();
    }
}
