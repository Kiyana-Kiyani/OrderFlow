using MassTransit;
using MediatR;
using OrderFlow.Application.Features.Orders.UpdatePaymentStatus;
using OrderFlow.Contracts.IntegrationEvents;

namespace OrderFlow.Infrastructure.Consumers
{
    public class PaymentFailedConsumer : IConsumer<PaymentFailedIntegrationEvent>
    {
        private readonly ISender _sender;

        public PaymentFailedConsumer(ISender sender)
        {
            _sender = sender;
        }

        public async Task Consume(ConsumeContext<PaymentFailedIntegrationEvent> context)
        {
            var message = context.Message;
            var command = new UpdatePaymentStatusCommand(message.OrderId, false);
            await _sender.Send(command);
        }
    }
}
