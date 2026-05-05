using OrderFlow.Contracts.IntegrationEvents;

namespace OrderFlow.Workers.Payment
{
    public class PaymentProcessor
    {
        public async Task HandleAsync(OrderPlacedIntegrationEvent orderPlacedEvent, CancellationToken cancellationToken)
        {
            // payment processing logic here
            Console.WriteLine(" work!! ");
            await Task.Delay(2000, cancellationToken);
        }
    }
}
