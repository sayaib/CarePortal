namespace CarePortal.Tests.Infrastructure;

public sealed class LocalPostgreSqlFactAttribute : FactAttribute
{
    public LocalPostgreSqlFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(LocalPostgreSqlSettings.ConnectionString))
        {
            Skip = $"Set {LocalPostgreSqlSettings.ConnectionStringEnvironmentVariable} to run local PostgreSQL integration tests.";
        }
    }
}
