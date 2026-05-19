namespace OrderFlow.Domain.Enums
{
    public enum OrderStatus
    {
        Created,          // Placed by customer
        Preparing,        // Kitchen is cooking
        ReadyForPickup,   // Food is ready on the counter
        OutForDelivery,   // Courier is driving
        Delivered,        // Food dropped off
        Cancelled         // General terminal state for cancellation
    }
}
