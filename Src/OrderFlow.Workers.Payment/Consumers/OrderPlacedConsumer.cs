using MassTransit;
using OrderFlow.Contracts.IntegrationEvents;

namespace OrderFlow.Workers.Payment.Consumers
{
    public class OrderPlacedConsumer : IConsumer<OrderPlacedIntegrationEvent>
    {
        private readonly ILogger<OrderPlacedConsumer> _logger;

        public OrderPlacedConsumer(ILogger<OrderPlacedConsumer> logger)
        {
            _logger = logger;
        }
        public async Task Consume(ConsumeContext<OrderPlacedIntegrationEvent> context)
        {
            _logger.LogInformation("zzzzzz");
            var message = context.Message;

            await Task.Delay(2000);
            bool paymentSuccess = new Random().Next(1, 100) > 10;

            if (paymentSuccess)
            {
                _logger.LogInformation("Payment successful for Order {OrderId}!", message.OrderId);
                await context.Publish(PaymentSucceededIntegrationEvent.CreateNew(message.OrderId)
                    , x => x.SetRoutingKey("order.placed.success"));
            }
            else
            {
                _logger.LogWarning("Payment failed for Order {OrderId}.", message.OrderId);
                await context.Publish(PaymentFailedIntegrationEvent.CreateNew(message.OrderId, "Declined by bank.")
                    , x => x.SetRoutingKey("order.placed.failed"));
            }
        }


    }
}


