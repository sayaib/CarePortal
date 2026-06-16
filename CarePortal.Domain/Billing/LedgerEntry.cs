using CarePortal.Domain.Common;

namespace CarePortal.Domain.Billing;

public sealed class LedgerEntry : Entity
{
    private LedgerEntry()
    {
    }

    private LedgerEntry(Guid invoiceId, Guid? lineItemId, LedgerEntryType type, decimal amount, DateTime createdAt)
    {
        InvoiceId = invoiceId;
        LineItemId = lineItemId;
        Type = type;
        Amount = amount;
        CreatedAt = createdAt;
    }

    public Guid InvoiceId { get; private set; }
    public Guid? LineItemId { get; private set; }
    public LedgerEntryType Type { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime CreatedAt { get; private set; }

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

        return new LedgerEntry(invoiceId, lineItemId, type, amount, DateTime.UtcNow);
    }

    public static LedgerEntry Create(Guid invoiceId, Guid? lineItemId, LedgerEntryType type, decimal amount, DateTime createdAt)
    {
        return CreateCore(invoiceId, lineItemId, type, amount, ToUtcDateTime(createdAt));
    }

    public static LedgerEntry PaymentReceived(Guid invoiceId, decimal amount, DateTime receivedAt)
    {
        return Create(invoiceId, null, LedgerEntryType.PaymentReceived, amount, receivedAt);
    }

    public static LedgerEntry Allocation(Guid invoiceId, Guid lineItemId, decimal amount, DateTime allocatedAt)
    {
        return Create(invoiceId, lineItemId, LedgerEntryType.Allocation, amount, allocatedAt);
    }

    public static LedgerEntry Credit(Guid invoiceId, decimal amount, DateTime creditedAt)
    {
        return Create(invoiceId, null, LedgerEntryType.Credit, amount, creditedAt);
    }

    private static LedgerEntry CreateCore(Guid invoiceId, Guid? lineItemId, LedgerEntryType type, decimal amount, DateTime createdAt)
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

        return new LedgerEntry(invoiceId, lineItemId, type, amount, createdAt);
    }

    private static DateTime ToUtcDateTime(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }
}
