namespace CarePortal.Domain.Billing;

public enum LedgerEntryType
{
    PaymentReceived = 1,
    Allocation = 2,
    Credit = 3,
    PaymentReversal = 4,
    AllocationReversal = 5,
    CreditReversal = 6
}
