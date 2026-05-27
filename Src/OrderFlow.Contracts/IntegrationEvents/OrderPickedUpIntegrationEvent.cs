namespace OrderFlow.Contracts.IntegrationEvents
{
    public record OrderPickedUpIntegrationEvent(
        Guid OrderId,
        Guid CustomerId,
        Guid CourierId,
        DateTime AcceptedAt);
}
