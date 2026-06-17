namespace CarePortal.Api.DTOs;

public class InvoiceDto
{
    public Guid Id { get; set; }
    public required string Reference { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<InvoiceLineItemDto> LineItems { get; set; } = [];
    public List<LedgerEntryDto> LedgerEntries { get; set; } = [];
}

public class InvoiceLineItemDto
{
    public Guid Id { get; set; }
    public required string Description { get; set; }
    public DateOnly DueDate { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class LedgerEntryDto
{
    public Guid Id { get; set; }
    public Guid? LineItemId { get; set; }
    public required string Type { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; }
}