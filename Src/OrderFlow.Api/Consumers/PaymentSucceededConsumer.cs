using MassTransit;
using MediatR;
using OrderFlow.Application.Features.Orders.UpdateOrderStatus;
using OrderFlow.Contracts.IntegrationEvents;

namespace OrderFlow.Api.Consumers
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
            var command = new UpdateOrderStatusCommand(message.OrderId);
            await _sender.Send(command);
        }
    }
}
