namespace CarePortal.Application.Abstractions.Billing;

public interface IBillingPaymentAllocator
{
    Task<decimal> AllocatePayment(Guid invoiceId, decimal paymentAmount, DateTime receivedAt);
    Task<decimal> ReversePayment(Guid paymentEntryId, DateTime reversedAt);
}
