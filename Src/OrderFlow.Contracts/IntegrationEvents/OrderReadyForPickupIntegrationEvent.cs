namespace OrderFlow.Contracts.IntegrationEvents
{
    public record OrderReadyForPickupIntegrationEvent(Guid OrderId, Guid RestaurantId, string RestaurantName, DateTime ReadyAt) : IIntegrationEvent
    {
        public Guid EventId => throw new NotImplementedException();

        public DateTime OccurredOnUtc => throw new NotImplementedException();
    }
}
