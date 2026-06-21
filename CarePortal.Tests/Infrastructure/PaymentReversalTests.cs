using CarePortal.Domain.Billing;
using CarePortal.Infrastructure.Billing;
using CarePortal.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CarePortal.Tests.Infrastructure;

public sealed class PaymentReversalTests
{
    [Fact]
    public async Task ReversePayment_Scenario1_ReversesFullAllocationAndRestoresBalance()
    {
        await using var fixture = await BillingDbFixture.CreateAsync();
        var invoice = Invoice.Create("REV-001");
        var lineItem = invoice.AddLineItem("Service", new DateOnly(2026, 1, 1), 100m);
        fixture.DbContext.Invoices.Add(invoice);
        await fixture.DbContext.SaveChangesAsync();

        var allocator = new EfCoreBillingPaymentAllocator(fixture.DbContext);
        var receivedAt = new DateTime(2026, 3, 1, 10, 0, 0, DateTimeKind.Utc);
        
        var balanceAfterAllocation = await allocator.AllocatePayment(invoice.Id, 100m, receivedAt);
        Assert.Equal(0m, balanceAfterAllocation);

        var paymentEntry = await fixture.DbContext.LedgerEntries
            .SingleAsync(e => e.Type == LedgerEntryType.PaymentReceived);

        var reversedAt = new DateTime(2026, 3, 2, 10, 0, 0, DateTimeKind.Utc);
        var balanceAfterReversal = await allocator.ReversePayment(paymentEntry.Id, reversedAt);

        Assert.Equal(100m, balanceAfterReversal);

        var ledgerEntries = await fixture.DbContext.LedgerEntries
            .Where(e => e.InvoiceId == invoice.Id)
            .ToListAsync();

        Assert.Contains(ledgerEntries, e => e.Type == LedgerEntryType.PaymentReversal && e.Amount == 100m);
        Assert.Contains(ledgerEntries, e => e.Type == LedgerEntryType.AllocationReversal && e.Amount == 100m && e.LineItemId == lineItem.Id);
        Assert.Equal(4, ledgerEntries.Count);
    }

    [Fact]
    public async Task ReversePayment_Scenario2_ReversesOverpaymentWithCredit()
    {
        await using var fixture = await BillingDbFixture.CreateAsync();
        var invoice = Invoice.Create("REV-002");
        invoice.AddLineItem("Service", new DateOnly(2026, 1, 1), 100m);
        fixture.DbContext.Invoices.Add(invoice);
        await fixture.DbContext.SaveChangesAsync();

        var allocator = new EfCoreBillingPaymentAllocator(fixture.DbContext);
        var receivedAt = new DateTime(2026, 3, 1, 10, 0, 0, DateTimeKind.Utc);
        
        var balanceAfterAllocation = await allocator.AllocatePayment(invoice.Id, 150m, receivedAt);
        Assert.Equal(-50m, balanceAfterAllocation);

        var paymentEntry = await fixture.DbContext.LedgerEntries
            .SingleAsync(e => e.Type == LedgerEntryType.PaymentReceived);

        var reversedAt = new DateTime(2026, 3, 2, 10, 0, 0, DateTimeKind.Utc);
        var balanceAfterReversal = await allocator.ReversePayment(paymentEntry.Id, reversedAt);

        Assert.Equal(100m, balanceAfterReversal);

        var ledgerEntries = await fixture.DbContext.LedgerEntries
            .Where(e => e.InvoiceId == invoice.Id)
            .ToListAsync();

        Assert.Contains(ledgerEntries, e => e.Type == LedgerEntryType.CreditReversal && e.Amount == 50m);
        Assert.Contains(ledgerEntries, e => e.Type == LedgerEntryType.PaymentReversal && e.Amount == 150m);
    }

    private sealed class BillingDbFixture : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;

        private BillingDbFixture(SqliteConnection connection, CarePortalDbContext dbContext)
        {
            _connection = connection;
            DbContext = dbContext;
        }

        public CarePortalDbContext DbContext { get; }

        public static async Task<BillingDbFixture> CreateAsync()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();

            var options = new DbContextOptionsBuilder<CarePortalDbContext>()
                .UseSqlite(connection)
                .Options;

            var dbContext = new CarePortalDbContext(options);
            await dbContext.Database.EnsureCreatedAsync();

            return new BillingDbFixture(connection, dbContext);
        }

        public async ValueTask DisposeAsync()
        {
            await DbContext.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }
}
