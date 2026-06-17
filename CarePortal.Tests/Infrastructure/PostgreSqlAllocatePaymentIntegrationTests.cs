using CarePortal.Domain.Billing;
using CarePortal.Infrastructure.Billing;
using CarePortal.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarePortal.Tests.Infrastructure;

public sealed class PostgreSqlAllocatePaymentIntegrationTests
{
    [LocalPostgreSqlFact]
    public async Task AllocatePayment_RunsAgainstLocalPostgreSqlAndPersistsExpectedLedgerEntries()
    {
        await ResetDatabaseAsync();

        await using var seedContext = CreateDbContext();
        var invoice = Invoice.Create("PG-ALLOC-001");
        var first = invoice.AddLineItem("Line item 1", new DateOnly(2026, 1, 1), 100m);
        var second = invoice.AddLineItem("Line item 2", new DateOnly(2026, 2, 1), 150.50m);
        var third = invoice.AddLineItem("Line item 3", new DateOnly(2026, 3, 1), 200.25m);

        seedContext.Invoices.Add(invoice);
        await seedContext.SaveChangesAsync();

        await using var allocationContext = CreateDbContext();
        var allocator = new EfCoreBillingPaymentAllocator(allocationContext);
        var receivedAt = new DateTime(2026, 4, 1, 9, 30, 0, DateTimeKind.Utc);

        var balance = await allocator.AllocatePayment(invoice.Id, 500m, receivedAt);

        await using var verificationContext = CreateDbContext();
        var ledgerEntries = await verificationContext.LedgerEntries
            .AsNoTracking()
            .Where(entry => entry.InvoiceId == invoice.Id)
            .ToListAsync();

        var payment = Assert.Single(ledgerEntries, entry => entry.Type == LedgerEntryType.PaymentReceived);
        var credit = Assert.Single(ledgerEntries, entry => entry.Type == LedgerEntryType.Credit);
        var allocations = ledgerEntries
            .Where(entry => entry.Type == LedgerEntryType.Allocation)
            .ToDictionary(entry => entry.LineItemId!.Value, entry => entry.Amount);

        Assert.Equal(-49.25m, balance);

        Assert.Equal(500m, payment.Amount);
        Assert.Null(payment.LineItemId);
        Assert.Equal(receivedAt, payment.CreatedAt);

        Assert.Equal(49.25m, credit.Amount);
        Assert.Null(credit.LineItemId);
        Assert.Equal(receivedAt, credit.CreatedAt);

        Assert.Equal(100m, allocations[first.Id]);
        Assert.Equal(150.50m, allocations[second.Id]);
        Assert.Equal(200.25m, allocations[third.Id]);
        Assert.Equal(5, ledgerEntries.Count);
    }

    [LocalPostgreSqlFact]
    public async Task AllocatePayment_RunsMigrationsAgainstLocalPostgreSqlAndLeavesDatabaseAtZeroBalanceForExactPayment()
    {
        await ResetDatabaseAsync();

        await using var seedContext = CreateDbContext();
        var invoice = Invoice.Create("PG-ALLOC-002");
        var first = invoice.AddLineItem("Line item 1", new DateOnly(2026, 1, 1), 100m);
        var second = invoice.AddLineItem("Line item 2", new DateOnly(2026, 2, 1), 150.50m);
        var third = invoice.AddLineItem("Line item 3", new DateOnly(2026, 3, 1), 200.25m);

        seedContext.Invoices.Add(invoice);
        await seedContext.SaveChangesAsync();

        await using var allocationContext = CreateDbContext();
        var allocator = new EfCoreBillingPaymentAllocator(allocationContext);

        var balance = await allocator.AllocatePayment(
            invoice.Id,
            450.75m,
            new DateTime(2026, 4, 1, 9, 30, 0, DateTimeKind.Utc));

        await using var verificationContext = CreateDbContext();
        var ledgerEntries = await verificationContext.LedgerEntries
            .AsNoTracking()
            .Where(entry => entry.InvoiceId == invoice.Id)
            .ToListAsync();

        var allocations = ledgerEntries
            .Where(entry => entry.Type == LedgerEntryType.Allocation)
            .ToDictionary(entry => entry.LineItemId!.Value, entry => entry.Amount);

        Assert.Equal(0m, balance);
        Assert.Single(ledgerEntries, entry => entry.Type == LedgerEntryType.PaymentReceived);
        Assert.DoesNotContain(ledgerEntries, entry => entry.Type == LedgerEntryType.Credit);
        Assert.Equal(100m, allocations[first.Id]);
        Assert.Equal(150.50m, allocations[second.Id]);
        Assert.Equal(200.25m, allocations[third.Id]);
        Assert.Equal(4, ledgerEntries.Count);
    }

    private CarePortalDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<CarePortalDbContext>()
            .UseNpgsql(LocalPostgreSqlSettings.ConnectionString, npgsql => npgsql.EnableRetryOnFailure())
            .Options;

        return new CarePortalDbContext(options);
    }

    private async Task ResetDatabaseAsync()
    {
        await using var dbContext = CreateDbContext();
        await dbContext.Database.MigrateAsync();
        await dbContext.Database.ExecuteSqlRawAsync(
            "TRUNCATE TABLE ledger_entries, invoice_line_items, invoices RESTART IDENTITY CASCADE;");
    }
}
