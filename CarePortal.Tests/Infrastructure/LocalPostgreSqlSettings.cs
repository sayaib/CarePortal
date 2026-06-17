namespace CarePortal.Tests.Infrastructure;

public static class LocalPostgreSqlSettings
{
    public const string ConnectionStringEnvironmentVariable = "CAREPORTAL_TEST_POSTGRES";

    public static string? ConnectionString =>
        Environment.GetEnvironmentVariable(ConnectionStringEnvironmentVariable);
}
