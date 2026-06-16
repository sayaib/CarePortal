using System.Data;
using CarePortal.Application.Abstractions.Billing;
using CarePortal.Domain.Billing;
using CarePortal.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CarePortal.Infrastructure.Billing;

public sealed class EfCoreBillingPaymentAllocator : IBillingPaymentAllocator
{
    private readonly CarePortalDbContext _dbContext;

    public EfCoreBillingPaymentAllocator(CarePortalDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<decimal> AllocatePayment(Guid invoiceId, decimal paymentAmount, DateTime receivedAt)
    {
        if (invoiceId == Guid.Empty)
        {
            throw new ArgumentException("Invoice id is required.", nameof(invoiceId));
        }

        if (paymentAmount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(paymentAmount), paymentAmount, "Payment amount must be greater than zero.");
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        var invoiceExists = await _dbContext.Invoices
            .AsNoTracking()
            .AnyAsync(invoice => invoice.Id == invoiceId);

        if (!invoiceExists)
        {
            throw new InvalidOperationException($"Invoice '{invoiceId}' was not found.");
        }

        _dbContext.LedgerEntries.Add(LedgerEntry.PaymentReceived(invoiceId, paymentAmount, receivedAt));

        var lineItems = await _dbContext.InvoiceLineItems
            .AsNoTracking()
            .Where(lineItem => lineItem.InvoiceId == invoiceId)
            .OrderBy(lineItem => lineItem.DueDate)
            .ThenBy(lineItem => lineItem.CreatedAt)
            .Select(lineItem => new
            {
                lineItem.Id,
                lineItem.Amount
            })
            .ToListAsync();

        var existingAllocations = await _dbContext.LedgerEntries
            .AsNoTracking()
            .Where(entry =>
                entry.InvoiceId == invoiceId &&
                entry.LineItemId != null &&
                entry.Type == LedgerEntryType.Allocation)
            .Select(entry => new { entry.LineItemId, entry.Amount })
            .ToListAsync();

        var allocatedByLineItem = existingAllocations
            .GroupBy(entry => entry.LineItemId!.Value)
            .ToDictionary(group => group.Key, group => group.Sum(entry => entry.Amount));

        var remainingPayment = paymentAmount;

        foreach (var lineItem in lineItems)
        {
            if (remainingPayment <= 0)
            {
                break;
            }

            allocatedByLineItem.TryGetValue(lineItem.Id, out var allocated);
            var outstanding = lineItem.Amount - allocated;
            if (outstanding <= 0)
            {
                continue;
            }

            var allocationAmount = Math.Min(outstanding, remainingPayment);
            _dbContext.LedgerEntries.Add(LedgerEntry.Allocation(invoiceId, lineItem.Id, allocationAmount, receivedAt));
            remainingPayment -= allocationAmount;
        }

        if (remainingPayment > 0)
        {
            _dbContext.LedgerEntries.Add(LedgerEntry.Credit(invoiceId, remainingPayment, receivedAt));
        }

        await _dbContext.SaveChangesAsync();

        var outstandingBalance = await CalculateOutstandingBalance(invoiceId);
        await transaction.CommitAsync();

        return outstandingBalance;
    }

    private async Task<decimal> CalculateOutstandingBalance(Guid invoiceId)
    {
        var lineItemAmounts = await _dbContext.InvoiceLineItems
            .AsNoTracking()
            .Where(lineItem => lineItem.InvoiceId == invoiceId)
            .Select(lineItem => lineItem.Amount)
            .ToListAsync();

        var ledgerEntries = await _dbContext.LedgerEntries
            .AsNoTracking()
            .Where(entry =>
                entry.InvoiceId == invoiceId &&
                (entry.Type == LedgerEntryType.Allocation || entry.Type == LedgerEntryType.Credit))
            .Select(entry => new { entry.Type, entry.Amount })
            .ToListAsync();

        var totalLineItemAmounts = lineItemAmounts.Sum();
        var totalAllocations = ledgerEntries
            .Where(entry => entry.Type == LedgerEntryType.Allocation)
            .Sum(entry => entry.Amount);
        var totalCredits = ledgerEntries
            .Where(entry => entry.Type == LedgerEntryType.Credit)
            .Sum(entry => entry.Amount);

        return totalLineItemAmounts - totalAllocations - totalCredits;
    }
}
