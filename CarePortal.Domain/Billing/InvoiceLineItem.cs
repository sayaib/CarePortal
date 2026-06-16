using CarePortal.Domain.Common;

namespace CarePortal.Domain.Billing;

public sealed class InvoiceLineItem : Entity
{
    private readonly List<LedgerEntry> _ledgerEntries = [];

    private InvoiceLineItem()
    {
        Description = string.Empty;
    }

    private InvoiceLineItem(Guid invoiceId, string description, DateOnly dueDate, decimal amount)
    {
        InvoiceId = invoiceId;
        Description = description.Trim();
        DueDate = dueDate;
        Amount = amount;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid InvoiceId { get; private set; }
    public string Description { get; private set; }
    public DateOnly DueDate { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public Invoice Invoice { get; private set; } = null!;
    public IReadOnlyCollection<LedgerEntry> LedgerEntries => _ledgerEntries.AsReadOnly();

    public static InvoiceLineItem Create(Guid invoiceId, string description, DateOnly dueDate, decimal amount)
    {
        if (invoiceId == Guid.Empty)
        {
            throw new ArgumentException("Invoice id is required.", nameof(invoiceId));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Description is required.", nameof(description));
        }

        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "Line item amount must be greater than zero.");
        }

        return new InvoiceLineItem(invoiceId, description, dueDate, amount);
    }
}
