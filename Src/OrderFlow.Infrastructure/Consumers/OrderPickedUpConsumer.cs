using MassTransit;
using OrderFlow.Application.Abstractions;
using OrderFlow.Contracts.IntegrationEvents;

namespace OrderFlow.Infrastructure.Consumers
{
    public class OrderPickedUpConsumer : IConsumer<OrderPickedUpIntegrationEvent>
    {
        private readonly IOrderNotificationService _notificationService;

        public OrderPickedUpConsumer(IOrderNotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public async Task Consume(ConsumeContext<OrderPickedUpIntegrationEvent> context)
        {
            var message = context.Message;

            await _notificationService.NotifyCustomerOfOrderStatusAsync(
                customerId: message.CustomerId,
                orderId: message.OrderId,
                status: "InTransit",
                message: "Courier is on the way.");
        }
    }
}
