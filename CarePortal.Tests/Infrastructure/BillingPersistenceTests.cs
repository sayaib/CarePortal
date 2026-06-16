using CarePortal.Domain.Billing;
using CarePortal.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarePortal.Tests.Infrastructure;

public sealed class BillingPersistenceTests
{
    [Fact]
    public async Task SaveChanges_PersistsInvoiceWithLineItemAndLedgerEntry()
    {
        var options = new DbContextOptionsBuilder<CarePortalDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new CarePortalDbContext(options);

        var invoice = Invoice.Create("INV-001");
        var lineItem = invoice.AddLineItem("Monthly care", DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)), 250.75m);
        var ledgerEntry = LedgerEntry.Create(invoice.Id, lineItem.Id, LedgerEntryType.Allocation, 250.75m);

        dbContext.Invoices.Add(invoice);
        dbContext.LedgerEntries.Add(ledgerEntry);
        await dbContext.SaveChangesAsync();

        var persisted = await dbContext.Invoices
            .Include(item => item.LineItems)
            .Include(item => item.LedgerEntries)
            .SingleAsync();

        Assert.Single(persisted.LineItems);
        Assert.Single(persisted.LedgerEntries);
    }

    [Fact]
    public async Task SaveChanges_PreventsLedgerEntryUpdates()
    {
        var options = new DbContextOptionsBuilder<CarePortalDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new CarePortalDbContext(options);
        var invoice = Invoice.Create("INV-002");
        var ledgerEntry = LedgerEntry.Create(invoice.Id, null, LedgerEntryType.PaymentReceived, 100m);

        dbContext.Invoices.Add(invoice);
        dbContext.LedgerEntries.Add(ledgerEntry);
        await dbContext.SaveChangesAsync();

        dbContext.Entry(ledgerEntry).State = EntityState.Modified;

        await Assert.ThrowsAsync<InvalidOperationException>(() => dbContext.SaveChangesAsync());
    }

    [Fact]
    public async Task SaveChanges_PreventsLedgerEntryDeletes()
    {
        var options = new DbContextOptionsBuilder<CarePortalDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var dbContext = new CarePortalDbContext(options);
        var invoice = Invoice.Create("INV-003");
        var ledgerEntry = LedgerEntry.Create(invoice.Id, null, LedgerEntryType.PaymentReceived, 100m);

        dbContext.Invoices.Add(invoice);
        dbContext.LedgerEntries.Add(ledgerEntry);
        await dbContext.SaveChangesAsync();

        dbContext.LedgerEntries.Remove(ledgerEntry);

        await Assert.ThrowsAsync<InvalidOperationException>(() => dbContext.SaveChangesAsync());
    }
}
