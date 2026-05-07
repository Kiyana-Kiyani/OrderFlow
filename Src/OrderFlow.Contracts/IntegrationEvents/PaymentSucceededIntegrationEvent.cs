namespace OrderFlow.Contracts.IntegrationEvents
{
    public record PaymentSucceededIntegrationEvent
        (Guid OrderId,
        Guid EventId,
        DateTime OccurredOnUtc) : IIntegrationEvent
    {
        public static PaymentSucceededIntegrationEvent CreateNew(Guid orderId)
        {
            return new PaymentSucceededIntegrationEvent(
                orderId,
                Guid.NewGuid(),
                DateTime.UtcNow);
        }
    }
}
