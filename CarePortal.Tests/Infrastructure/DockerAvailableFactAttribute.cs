using System.Diagnostics;

namespace CarePortal.Tests.Infrastructure;

public sealed class DockerAvailableFactAttribute : FactAttribute
{
    public DockerAvailableFactAttribute()
    {
        if (!IsDockerAvailable())
        {
            Skip = "Docker is required for PostgreSQL Testcontainers integration tests.";
        }
    }

    private static bool IsDockerAvailable()
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = "info --format \"{{.ServerVersion}}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });

            return process is not null &&
                   process.WaitForExit(3000) &&
                   process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}
