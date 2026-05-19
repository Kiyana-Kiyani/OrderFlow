namespace OrderFlow.Domain.Enums
{
    public enum PaymentStatus
    {
        Pending,          // Awaiting bank response
        Succeeded,        // Money securely captured
        Failed            // Card declined / Insufficient funds
    }
}
