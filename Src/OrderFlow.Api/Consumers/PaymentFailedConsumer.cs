using MassTransit;
using OrderFlow.Contracts.IntegrationEvents;

namespace OrderFlow.Api.Consumers
{
    public class PaymentFailedConsumer : IConsumer<PaymentFailedIntegrationEvent>
    {
        ILogger<PaymentFailedConsumer> _logger;

        public PaymentFailedConsumer(ILogger<PaymentFailedConsumer> logger)
        {
            _logger = logger;
        }
        public Task Consume(ConsumeContext<PaymentFailedIntegrationEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation("PaymentFailedConsumer created {0}", message);
            return Task.CompletedTask;
        }
    }
}
