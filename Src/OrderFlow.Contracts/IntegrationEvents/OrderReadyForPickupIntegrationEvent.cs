namespace OrderFlow.Contracts.IntegrationEvents
{
    public record OrderReadyForPickupIntegrationEvent
        (Guid OrderId, Guid RestaurantId, string RestaurantName, DateTime ReadyAt,
        string ResturantAddress, double ResturantLatitude, double ResturantLongitude
        , string CustomerAddress, double CustomerLatitude, double CustomerLongitude
        , Guid EventId, DateTime OccurredOnUtc)
        : IIntegrationEvent
    {
        public static OrderReadyForPickupIntegrationEvent Create(Guid orderId, Guid restaurantId, string restaurantName, DateTime readyAt,
            string restaurantAddress, double restaurantLatitude, double restaurantLongitude,
            string customerAddress, double customerLatitude, double customerLongitude)
           => new OrderReadyForPickupIntegrationEvent(orderId, restaurantId, restaurantName, readyAt,
               restaurantAddress, restaurantLatitude, restaurantLongitude,
               customerAddress, customerLatitude, customerLongitude, Guid.NewGuid(), DateTime.UtcNow);
    }
}
