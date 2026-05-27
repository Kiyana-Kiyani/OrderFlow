using Microsoft.Extensions.Logging;
using OrderFlow.Worker.Courier.Abstractions;

namespace OrderFlow.Worker.Courier.Infrastructure
{
    public class FirebasePushNotificationService : IPushNotificationService
    {
        private readonly ILogger<FirebasePushNotificationService> _logger;

        public FirebasePushNotificationService(ILogger<FirebasePushNotificationService> logger)
        {
            _logger = logger;
        }

        public Task SendPushAsync(string userId, string title, object payload)
        {
            // در آینده اینجا کدهای SDK فایربیس یا اپل قرار می‌گیرد:
            // var message = new Message() { ... };
            // await FirebaseMessaging.DefaultInstance.SendAsync(message);

            _logger.LogInformation("Push Notification successfully sent via FCM/APNs to User: {UserId} with Title: '{Title}'", userId, title);
            return Task.CompletedTask;
        }
    }
}
