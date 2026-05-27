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

            // صدا زدن متد زنده سیگنال‌آر برای آپدیت مانیتور مشتری
            await _notificationService.NotifyCustomerOfOrderStatusAsync(
                message.CustomerId,
                message.OrderId,
                status: "InTransit",
                message: "Courier is on the way.");
        }
    }
}
