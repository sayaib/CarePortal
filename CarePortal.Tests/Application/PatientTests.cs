using CarePortal.Domain.Patients;

namespace CarePortal.Tests.Application;

public sealed class PatientTests
{
    [Fact]
    public void Create_NormalizesEmail()
    {
        var patient = Patient.Create("Ada", "Lovelace", " ADA@example.COM ", new DateOnly(1815, 12, 10));

        Assert.Equal("ada@example.com", patient.Email);
    }

    [Fact]
    public void Create_RejectsFutureDateOfBirth()
    {
        var futureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

        Assert.Throws<ArgumentException>(() => Patient.Create("Ada", "Lovelace", "ada@example.com", futureDate));
    }
}
