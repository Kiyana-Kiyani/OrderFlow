using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using OrderFlow.Contracts.Hubs;
using OrderFlow.Contracts.IntegrationEvents;
using OrderFlow.Worker.Courier.Abstractions;
using StackExchange.Redis;

namespace OrderFlow.Worker.Courier.Consumers
{
    public class OrderReadyForPickupConsumer : IConsumer<OrderReadyForPickupIntegrationEvent>
    {
        private readonly IDatabase _redisDb;
        private readonly IHubContext<OrderHub> _hubContext;
        private readonly IPushNotificationService _pushNotificationService; // 👈 اضافه شد
        private readonly ILogger<OrderReadyForPickupConsumer> _logger;

        public OrderReadyForPickupConsumer(
            IConnectionMultiplexer redisConnection,
            IHubContext<OrderHub> hubContext,
            IPushNotificationService pushNotificationService, // 👈 تزریق سرویس جدید
            ILogger<OrderReadyForPickupConsumer> logger)
        {
            _redisDb = redisConnection.GetDatabase();
            _hubContext = hubContext;
            _pushNotificationService = pushNotificationService;
            _logger = logger;
        }
        public async Task Consume(ConsumeContext<OrderReadyForPickupIntegrationEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation("Worker received OrderReady event for Order ID: {OrderId}", message.OrderId);

            string geoKey = "couriers:locations";
            double restaurantLng = message.RestaurantLongitude;
            double restaurantLat = message.RestaurantLatitude;

            // ۱. استفاده از قابلیت شگفت‌انگیز Redis Geo برای فیلتر پیک‌ها تا شعاع ۵ کیلومتری رستوران
            var nearbyCouriers = await _redisDb.GeoSearchAsync(geoKey, restaurantLng, restaurantLat, new GeoSearchCircle(3, GeoUnit.Kilometers));
            if (nearbyCouriers.Length == 0)
            {
                _logger.LogWarning("No couriers found within 3km radius for Order {OrderId}.", message.OrderId);
                nearbyCouriers = await _redisDb.GeoSearchAsync(geoKey, restaurantLng, restaurantLat, new GeoSearchCircle(6, GeoUnit.Kilometers));

            }
            _logger.LogInformation("Found {Count} couriers within 5km. Assessing presence status...", nearbyCouriers.Length);

            var notificationPayload = new
            {
                OrderId = message.OrderId,
                RestaurantName = message.RestaurantName,
                RestaurantAddress = message.RestaurantAddress,
                DeliveryAddress = message.CustomerAddress
            };


            foreach (var courierResult in nearbyCouriers)
            {
                string courierId = courierResult.Member.ToString()!;
                string presenceKey = $"presence:courier:{courierId}";

                // ۲. استعلام وضعیت زنده بودن وب‌سوکت پیک از ردیس حضور (Presence)
                string? presenceStatus = await _redisDb.StringGetAsync(presenceKey);
                bool isOnline = presenceStatus == "Online";

                if (isOnline)
                {
                    // 🚀 حالت اول: پیک آنلاین است (اپ باز است) -> شلیک مستقیم روی وب‌سوکت از طریق پروکسی
                    _logger.LogInformation("Courier {CourierId} is Online. Dispatching via SignalR Proxy...", courierId);
                    //await _hubContext.Clients.All.SendAsync("ReceiveOrderNotification", new
                    //{
                    //    OrderId = message.OrderId,
                    //    RestaurantName = message.RestaurantName,
                    //    Status = "ReadyForPickup",
                    //    Message = "ready for pickup"
                    //});
                    // پیام از ربیت‌ام‌کی عبور کرده و روی گوشی پیک پاپ‌آپ می‌شود
                    await _hubContext.Clients.Group($"Courier_{courierId}")
                        .SendAsync("ReceiveAvailableOrder", notificationPayload);
                }
                else
                {
                    // 📱 حالت دوم: پیک آفلاین است (اپ بسته است) -> شلیک مستقیم پُش‌نوتیفیکیشن به گوشی
                    _logger.LogInformation("Courier {CourierId} is Offline. Forwarding to Push Notification Service...", courierId);

                    // اجرای قطعی سرویس پُش نوتیفیکیشن فایربیس/اپل
                    await _pushNotificationService.SendPushAsync(
                        courierId,
                        "Order is Ready near you.",
                        notificationPayload);
                }

            }
        }
    }
}
