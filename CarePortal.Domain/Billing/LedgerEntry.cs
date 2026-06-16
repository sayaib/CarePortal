using CarePortal.Domain.Common;

namespace CarePortal.Domain.Billing;

public sealed class LedgerEntry : Entity
{
    private LedgerEntry()
    {
    }

    private LedgerEntry(Guid invoiceId, Guid? lineItemId, LedgerEntryType type, decimal amount)
    {
        InvoiceId = invoiceId;
        LineItemId = lineItemId;
        Type = type;
        Amount = amount;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    public Guid InvoiceId { get; private set; }
    public Guid? LineItemId { get; private set; }
    public LedgerEntryType Type { get; private set; }
    public decimal Amount { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public Invoice Invoice { get; private set; } = null!;
    public InvoiceLineItem? LineItem { get; private set; }

    public static LedgerEntry Create(Guid invoiceId, Guid? lineItemId, LedgerEntryType type, decimal amount)
    {
        if (invoiceId == Guid.Empty)
        {
            throw new ArgumentException("Invoice id is required.", nameof(invoiceId));
        }

        if (lineItemId == Guid.Empty)
        {
            throw new ArgumentException("Line item id cannot be empty when provided.", nameof(lineItemId));
        }

        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "Ledger amount must be greater than zero.");
        }

        return new LedgerEntry(invoiceId, lineItemId, type, amount);
    }
}
