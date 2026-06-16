using CarePortal.Domain.Billing;
using CarePortal.Infrastructure.Billing;
using CarePortal.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CarePortal.Tests.Infrastructure;

public sealed class BillingAllocationTests
{
    [Fact]
    public async Task AllocatePayment_AllocatesOldestLineItemsFirst()
    {
        await using var fixture = await BillingDbFixture.CreateAsync();
        var invoice = Invoice.Create("ALLOC-001");
        var first = invoice.AddLineItem("Oldest", new DateOnly(2026, 1, 1), 100m);
        var second = invoice.AddLineItem("Newest", new DateOnly(2026, 2, 1), 75m);

        fixture.DbContext.Invoices.Add(invoice);
        await fixture.DbContext.SaveChangesAsync();

        var allocator = new EfCoreBillingPaymentAllocator(fixture.DbContext);
        var balance = await allocator.AllocatePayment(invoice.Id, 125m, new DateTime(2026, 3, 1, 10, 0, 0, DateTimeKind.Utc));

        var allocations = await fixture.DbContext.LedgerEntries
            .Where(entry => entry.Type == LedgerEntryType.Allocation)
            .ToListAsync();
        var allocationsByLineItem = allocations.ToDictionary(entry => entry.LineItemId!.Value, entry => entry.Amount);

        Assert.Equal(50m, balance);
        Assert.Equal(100m, allocationsByLineItem[first.Id]);
        Assert.Equal(25m, allocationsByLineItem[second.Id]);
    }

    [Fact]
    public async Task AllocatePayment_CreatesCreditForOverpaymentAndReturnsNegativeBalance()
    {
        await using var fixture = await BillingDbFixture.CreateAsync();
        var invoice = Invoice.Create("ALLOC-002");
        invoice.AddLineItem("Care", new DateOnly(2026, 1, 1), 100m);

        fixture.DbContext.Invoices.Add(invoice);
        await fixture.DbContext.SaveChangesAsync();

        var allocator = new EfCoreBillingPaymentAllocator(fixture.DbContext);
        var balance = await allocator.AllocatePayment(invoice.Id, 125m, new DateTime(2026, 3, 1, 10, 0, 0, DateTimeKind.Utc));

        var credit = await fixture.DbContext.LedgerEntries.SingleAsync(entry => entry.Type == LedgerEntryType.Credit);

        Assert.Equal(-25m, balance);
        Assert.Null(credit.LineItemId);
        Assert.Equal(25m, credit.Amount);
    }

    [Fact]
    public async Task AllocatePayment_InsertsPaymentReceivedLedgerEntry()
    {
        await using var fixture = await BillingDbFixture.CreateAsync();
        var receivedAt = new DateTime(2026, 3, 1, 10, 0, 0, DateTimeKind.Utc);
        var invoice = Invoice.Create("ALLOC-003");
        invoice.AddLineItem("Care", new DateOnly(2026, 1, 1), 100m);

        fixture.DbContext.Invoices.Add(invoice);
        await fixture.DbContext.SaveChangesAsync();

        var allocator = new EfCoreBillingPaymentAllocator(fixture.DbContext);
        await allocator.AllocatePayment(invoice.Id, 60m, receivedAt);

        var payment = await fixture.DbContext.LedgerEntries.SingleAsync(entry => entry.Type == LedgerEntryType.PaymentReceived);

        Assert.Null(payment.LineItemId);
        Assert.Equal(60m, payment.Amount);
        Assert.Equal(receivedAt, payment.CreatedAt);
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
