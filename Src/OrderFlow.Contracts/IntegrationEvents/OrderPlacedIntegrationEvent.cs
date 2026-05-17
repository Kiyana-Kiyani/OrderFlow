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
        public static OrderPlacedIntegrationEvent CreateNew(Guid orderId, Guid customerUserId, Guid restaurantId, decimal totalAmount)
        {
            return new OrderPlacedIntegrationEvent(
                orderId,
                customerUserId,
                restaurantId,
                totalAmount,
                Guid.NewGuid(),
                DateTime.UtcNow);
        }
    }
}
