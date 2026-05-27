using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using OrderFlow.Contracts;
using OrderFlow.Contracts.IntegrationEvents;
using OrderFlow.Worker.Courier.Hubs;

namespace OrderFlow.Worker.Courier.Consumers
{
    public class FoodReadyConsumer : IConsumer<OrderReadyForPickupIntegrationEvent>
    {
        private readonly ICourierConnectionTracker _connectionTracker;
        private readonly IHubContext<OrderNotificationHub> _hubContext;
        private readonly ILogger<FoodReadyConsumer> _logger;

        public FoodReadyConsumer(
            ICourierConnectionTracker connectionTracker,
            IHubContext<OrderNotificationHub> hubContext,
            ILogger<FoodReadyConsumer> logger)
        {
            _connectionTracker = connectionTracker;
            _hubContext = hubContext;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<OrderReadyForPickupIntegrationEvent> context)
        {
            OrderReadyForPickupIntegrationEvent message = context.Message;

            _logger.LogInformation("Processing food readiness event for Order ID: {OrderId} from Restaurant: {RestaurantName}",
                message.OrderId, message.RestaurantName);

            // Simulation placeholder: Assume our business rule logistics algorithm matched this driver ID:
            Guid targetCourierId = Guid.Parse("00000000-0000-0000-0000-000000000000");

            // Query the Redis whiteboard using our abstraction infrastructure layer
            string? connectionId = await _connectionTracker.GetConnectionIdAsync(targetCourierId);

            if (!string.IsNullOrEmpty(connectionId))
            {
                _logger.LogInformation("Courier {CourierId} detected online. Routing message through SignalR backplane.", targetCourierId);

                // Publish to the shared Redis Pub/Sub radio line. 
                // The Web API app will pick this up from Redis and push it to the user's active socket channel.
                await _hubContext.Clients.Client(connectionId).SendAsync("AvailableJobAlert", new
                {
                    OrderId = message.OrderId,
                    RestaurantName = message.RestaurantName
                });
            }
            else
            {
                _logger.LogInformation("Courier {CourierId} is offline. Dispatched to mobile phone OS Push Notification Network.", targetCourierId);

                // Execute fallback push routing channels
                await SendPlatformPushNotificationAsync(targetCourierId, message.RestaurantName);
            }
        }

        private Task SendPlatformPushNotificationAsync(Guid courierId, string restaurantName)
        {
            // This is where your external Firebase (FCM) or Apple (APNS) payload clients execute
            _logger.LogInformation("Outbound raw notification envelope successfully queued for FCM/APNS proxy transport hubs.");
            return Task.CompletedTask;
        }
    }
}
