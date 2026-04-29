namespace OrderFlow.Contracts.IntegrationEvents
{
    public record OrderPlacedIntegrationEvent
       (Guid OrderId,
        Guid CustomerUserId,
        Guid RestaurantId,
        decimal TotalAmount,
        Guid EventId,
        DateTime OccurredOnUtc) : IIntegrationEvent
    {
        public OrderPlacedIntegrationEvent(Guid orderId, Guid customerUserId, Guid restaurantId, decimal totalAmount)
        : this(orderId, customerUserId, restaurantId, totalAmount, Guid.NewGuid(), DateTime.UtcNow)
        {
        }
    }
}
