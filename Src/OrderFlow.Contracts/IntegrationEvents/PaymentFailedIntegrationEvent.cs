namespace OrderFlow.Contracts.IntegrationEvents
{
    public record PaymentFailedIntegrationEvent(Guid OrderId, Guid EventId, string Reason, DateTime OccurredOnUtc) : IIntegrationEvent
    {
        public static PaymentFailedIntegrationEvent CreateNew(Guid orderId, string reason)
        {
            return new PaymentFailedIntegrationEvent(
                orderId,
                Guid.NewGuid(),
                reason,
                DateTime.UtcNow);
        }
    }
}
