using MassTransit;
using MediatR;
using OrderFlow.Application.Features.Orders.UpdatePaymentStatus;
using OrderFlow.Contracts.IntegrationEvents;

namespace OrderFlow.Infrastructure.Consumers
{
    public class PaymentSucceededConsumer : IConsumer<PaymentSucceededIntegrationEvent>
    {
        private readonly ISender _sender;

        public PaymentSucceededConsumer(ISender sender)
        {
            _sender = sender;
        }

        public async Task Consume(ConsumeContext<PaymentSucceededIntegrationEvent> context)
        {
            var message = context.Message;
            var command = new UpdatePaymentStatusCommand(message.OrderId, true);
            await _sender.Send(command);
        }
    }
}
