using CarePortal.Domain.Billing;

namespace CarePortal.Tests.Application;

public sealed class BillingEntityTests
{
    [Fact]
    public void Invoice_Create_TrimsReference()
    {
        var invoice = Invoice.Create(" INV-001 ");

        Assert.Equal("INV-001", invoice.Reference);
    }

    [Fact]
    public void InvoiceLineItem_Create_RequiresPositiveAmount()
    {
        var invoiceId = Guid.NewGuid();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            InvoiceLineItem.Create(invoiceId, "Monthly care", DateOnly.FromDateTime(DateTime.UtcNow), 0m));
    }

    [Fact]
    public void LedgerEntry_Create_AllowsNullableLineItem()
    {
        var entry = LedgerEntry.Create(Guid.NewGuid(), null, LedgerEntryType.PaymentReceived, 100.50m);

        Assert.Null(entry.LineItemId);
        Assert.Equal(100.50m, entry.Amount);
    }
}
