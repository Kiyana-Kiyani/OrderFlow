namespace OrderFlow.Contracts.IntegrationEvents
{
    public record OrderReadyForPickupIntegrationEvent
        (Guid OrderId, Guid RestaurantId, string RestaurantName, DateTime ReadyAt
        , Guid EventId, DateTime OccurredOnUtc)
        : IIntegrationEvent
    {
        public static OrderReadyForPickupIntegrationEvent Create(Guid orderId, Guid restaurantId, string restaurantName, DateTime readyAt)
           => new OrderReadyForPickupIntegrationEvent(orderId, restaurantId, restaurantName, readyAt, Guid.NewGuid(), DateTime.UtcNow);
    }
}
