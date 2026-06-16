using System.Diagnostics;
using CarePortal.Domain.Billing;
using CarePortal.Infrastructure.Billing;
using CarePortal.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace CarePortal.Tests.Infrastructure;

public sealed class BillingAllocationPostgreSqlTests
{
    [PostgreSqlContainerFact]
    public async Task AllocatePayment_WithPostgreSqlContainer_RunsMigrationsAndPersistsExpectedLedgerState()
    {
        await using var postgres = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("careportal_tests")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .Build();

        await postgres.StartAsync();

        var options = new DbContextOptionsBuilder<CarePortalDbContext>()
            .UseNpgsql(postgres.GetConnectionString(), npgsql => npgsql.EnableRetryOnFailure())
            .Options;

        await using var dbContext = new CarePortalDbContext(options);
        await dbContext.Database.MigrateAsync();

        var invoice = Invoice.Create("PG-ALLOC-001");
        var first = invoice.AddLineItem("Line item 1", new DateOnly(2026, 1, 1), 100m);
        var second = invoice.AddLineItem("Line item 2", new DateOnly(2026, 2, 1), 150.50m);
        var third = invoice.AddLineItem("Line item 3", new DateOnly(2026, 3, 1), 200.25m);

        dbContext.Invoices.Add(invoice);
        await dbContext.SaveChangesAsync();

        var allocator = new EfCoreBillingPaymentAllocator(dbContext);
        var balance = await allocator.AllocatePayment(
            invoice.Id,
            500m,
            new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc));

        dbContext.ChangeTracker.Clear();

        var persistedInvoice = await dbContext.Invoices
            .Include(item => item.LineItems)
            .Include(item => item.LedgerEntries)
            .SingleAsync(item => item.Id == invoice.Id);

        var ledgerEntries = await dbContext.LedgerEntries
            .AsNoTracking()
            .Where(entry => entry.InvoiceId == invoice.Id)
            .ToListAsync();

        var payment = Assert.Single(ledgerEntries, entry => entry.Type == LedgerEntryType.PaymentReceived);
        var credit = Assert.Single(ledgerEntries, entry => entry.Type == LedgerEntryType.Credit);
        var allocations = ledgerEntries
            .Where(entry => entry.Type == LedgerEntryType.Allocation)
            .ToDictionary(entry => entry.LineItemId!.Value, entry => entry.Amount);

        Assert.Equal(3, persistedInvoice.LineItems.Count);
        Assert.Equal(5, persistedInvoice.LedgerEntries.Count);
        Assert.Equal(500m, payment.Amount);
        Assert.Null(payment.LineItemId);
        Assert.Equal(100m, allocations[first.Id]);
        Assert.Equal(150.50m, allocations[second.Id]);
        Assert.Equal(200.25m, allocations[third.Id]);
        Assert.Null(credit.LineItemId);
        Assert.Equal(49.25m, credit.Amount);
        Assert.Equal(-49.25m, balance);
    }
}

public sealed class PostgreSqlContainerFactAttribute : FactAttribute
{
    public PostgreSqlContainerFactAttribute()
    {
        if (!DockerIsAvailable())
        {
            Skip = "Docker is required for PostgreSQL Testcontainers integration tests.";
        }
    }

    private static bool DockerIsAvailable()
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = "version --format {{.Server.Version}}",
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            });

            if (process is null)
            {
                return false;
            }

            process.WaitForExit(5000);
            return process.HasExited && process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}
