using CarePortal.Api.DTOs;
using CarePortal.Application.Abstractions.Billing;
using CarePortal.Domain.Billing;
using CarePortal.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarePortal.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BillingController : ControllerBase
{
    private readonly CarePortalDbContext _dbContext;
    private readonly IBillingPaymentAllocator _paymentAllocator;

    public BillingController(CarePortalDbContext dbContext, IBillingPaymentAllocator paymentAllocator)
    {
        _dbContext = dbContext;
        _paymentAllocator = paymentAllocator;
    }

    [HttpGet("invoices")]
    public async Task<IActionResult> GetAllInvoices()
    {
        var invoices = await _dbContext.Invoices
            .Include(i => i.LineItems)
            .Include(i => i.LedgerEntries)
            .Select(i => MapToInvoiceDto(i))
            .ToListAsync();
        return Ok(invoices);
    }

    [HttpGet("invoices/{id:guid}")]
    public async Task<IActionResult> GetInvoice(Guid id)
    {
        var invoice = await _dbContext.Invoices
            .Include(i => i.LineItems)
            .Include(i => i.LedgerEntries)
            .FirstOrDefaultAsync(i => i.Id == id);
        if (invoice == null)
        {
            return NotFound();
        }
        return Ok(MapToInvoiceDto(invoice));
    }

    [HttpPost("invoices")]
    public async Task<IActionResult> CreateInvoice([FromBody] CreateInvoiceRequest request)
    {
        var invoice = Invoice.Create(request.Reference);
        foreach (var lineItem in request.LineItems)
        {
            invoice.AddLineItem(lineItem.Description, lineItem.DueDate, lineItem.Amount);
        }
        _dbContext.Invoices.Add(invoice);
        await _dbContext.SaveChangesAsync();
        return CreatedAtAction(nameof(GetInvoice), new { id = invoice.Id }, MapToInvoiceDto(invoice));
    }

    [HttpPost("invoices/{id:guid}/allocate-payment")]
    public async Task<IActionResult> AllocatePayment(Guid id, [FromBody] AllocatePaymentRequest request)
    {
        var newBalance = await _paymentAllocator.AllocatePayment(id, request.PaymentAmount, request.ReceivedAt);
        return Ok(new { NewBalance = newBalance });
    }

    [HttpGet("invoices/{id:guid}/balance")]
    public async Task<IActionResult> GetInvoiceBalance(Guid id)
    {
        var lineItemAmounts = await _dbContext.InvoiceLineItems
            .AsNoTracking()
            .Where(li => li.InvoiceId == id)
            .Select(li => li.Amount)
            .ToListAsync();

        var ledgerEntries = await _dbContext.LedgerEntries
            .AsNoTracking()
            .Where(entry =>
                entry.InvoiceId == id &&
                (entry.Type == LedgerEntryType.Allocation || entry.Type == LedgerEntryType.Credit))
            .Select(entry => new { entry.Type, entry.Amount })
            .ToListAsync();

        var totalLineItemAmounts = lineItemAmounts.Sum();
        var totalAllocations = ledgerEntries.Where(e => e.Type == LedgerEntryType.Allocation).Sum(e => e.Amount);
        var totalCredits = ledgerEntries.Where(e => e.Type == LedgerEntryType.Credit).Sum(e => e.Amount);

        var balance = totalLineItemAmounts - totalAllocations - totalCredits;
        return Ok(new { Balance = balance });
    }

    private static InvoiceDto MapToInvoiceDto(Invoice invoice)
    {
        return new InvoiceDto
        {
            Id = invoice.Id,
            Reference = invoice.Reference,
            CreatedAt = invoice.CreatedAt,
            LineItems = invoice.LineItems.Select(li => new InvoiceLineItemDto
            {
                Id = li.Id,
                Description = li.Description,
                DueDate = li.DueDate,
                Amount = li.Amount,
                CreatedAt = li.CreatedAt
            }).ToList(),
            LedgerEntries = invoice.LedgerEntries.Select(le => new LedgerEntryDto
            {
                Id = le.Id,
                LineItemId = le.LineItemId,
                Type = le.Type.ToString(),
                Amount = le.Amount,
                CreatedAt = le.CreatedAt
            }).ToList()
        };
    }
}

public class CreateInvoiceRequest
{
    public required string Reference { get; set; } = string.Empty;
    public required List<CreateInvoiceLineItemRequest> LineItems { get; set; } = [];
}

public class CreateInvoiceLineItemRequest
{
    public required string Description { get; set; } = string.Empty;
    public DateOnly DueDate { get; set; }
    public decimal Amount { get; set; }
}

public class AllocatePaymentRequest
{
    public decimal PaymentAmount { get; set; }
    public DateTime ReceivedAt { get; set; }
}