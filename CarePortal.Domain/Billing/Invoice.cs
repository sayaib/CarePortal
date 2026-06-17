using CarePortal.Domain.Common;

namespace CarePortal.Domain.Billing;

public sealed class Invoice : Entity
{
    private readonly List<InvoiceLineItem> _lineItems = [];
    private readonly List<LedgerEntry> _ledgerEntries = [];

    private Invoice()
    {
        Reference = string.Empty;
    }

    private Invoice(string reference)
    {
        Reference = reference.Trim();
        CreatedAt = DateTime.UtcNow;
    }

    public string Reference { get; private set; }
    public DateTime CreatedAt { get; private set; }
    // Concurrency token for optimistic concurrency control
    public byte[] RowVersion { get; private set; } = [];

    public IReadOnlyCollection<InvoiceLineItem> LineItems => _lineItems.AsReadOnly();
    public IReadOnlyCollection<LedgerEntry> LedgerEntries => _ledgerEntries.AsReadOnly();

    public static Invoice Create(string reference)
    {
        if (string.IsNullOrWhiteSpace(reference))
        {
            throw new ArgumentException("Invoice reference is required.", nameof(reference));
        }

        return new Invoice(reference);
    }

    public InvoiceLineItem AddLineItem(string description, DateOnly dueDate, decimal amount)
    {
        var lineItem = InvoiceLineItem.Create(Id, description, dueDate, amount);
        _lineItems.Add(lineItem);
        return lineItem;
    }
}
