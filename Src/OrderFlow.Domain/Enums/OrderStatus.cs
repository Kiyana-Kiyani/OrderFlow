namespace OrderFlow.Domain.Enums
{
    public enum OrderStatus
    {
        Created,
        Preparing,
        ReadyForPickup,
        OutForDelivery,
        Delivered,
        Cancelled
    }
}
