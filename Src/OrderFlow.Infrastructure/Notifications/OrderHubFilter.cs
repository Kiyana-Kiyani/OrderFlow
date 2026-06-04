using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Security.Claims;

namespace OrderFlow.Infrastructure.Notifications
{
    public class OrderHubFilter : IHubFilter
    {
        private readonly IDatabase _redisDb;
        private readonly ILogger<OrderHubFilter> _logger;
        public OrderHubFilter(IConnectionMultiplexer redisConnection, ILogger<OrderHubFilter> logger)
        {
            _redisDb = redisConnection.GetDatabase();
            _logger = logger;
        }
        public async Task OnConnectedAsync(HubLifetimeContext context, Func<HubLifetimeContext, Task> next)
        {
            if (context.Hub is Contracts.Hubs.OrderHub)
            {
                var userId = context.Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var roles = context.Context.User?.FindAll(ClaimTypes.Role);
                var restaurantId = context.Context.User?.FindFirst("RestaurantId")?.Value;
                _logger.LogInformation("SignalR Client Connecting. User: {UserId}", userId);

                if (roles == null)
                {
                    _logger.LogWarning("User {UserId} has no roles.", userId);
                    await next(context);
                    return;
                }

                foreach (var roleClaim in roles)
                {
                    var role = roleClaim.Value;
                    _logger.LogInformation("User {UserId} has role: {Role}", userId, roleClaim.Value);

                    if (role == "Courier" && !string.IsNullOrEmpty(userId))
                    {
                        var userIDLower = userId.ToLowerInvariant();
                        await context.Hub.Groups.AddToGroupAsync(context.Context.ConnectionId, "Couriers");
                        await context.Hub.Groups.AddToGroupAsync(context.Context.ConnectionId, $"Courier_{userIDLower}");

                        string presenceKey = $"presence:courier:{userIDLower}";
                        await _redisDb.StringSetAsync(presenceKey, "Online", TimeSpan.FromMinutes(30));
                    }
                    else if (role == "Owner" && !string.IsNullOrEmpty(restaurantId))
                    {
                        await context.Hub.Groups.AddToGroupAsync(context.Context.ConnectionId, $"Restaurant_{restaurantId}");
                    }
                    else if (role == "Customer" && !string.IsNullOrEmpty(userId))
                    {
                        await context.Hub.Groups.AddToGroupAsync(context.Context.ConnectionId, $"Customer_{userId}");
                    }
                    else if (role == "Admin")
                    {
                        await context.Hub.Groups.AddToGroupAsync(context.Context.ConnectionId, "Admins");
                    }
                }

            }
            await next(context);
        }

        public async Task OnDisconnectedAsync(HubLifetimeContext context, Exception? exception, Func<HubLifetimeContext, Exception?, Task> next)
        {
            if (context.Hub is Contracts.Hubs.OrderHub)
            {
                var userId = context.Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var role = context.Context.User?.FindFirst(ClaimTypes.Role)?.Value;

                if (role == "Courier" && !string.IsNullOrEmpty(userId))
                {
                    string presenceKey = $"presence:courier:{userId.ToLowerInvariant()}";
                    await _redisDb.KeyDeleteAsync(presenceKey);
                    _logger.LogInformation("Courier {UserId} disconnected. Presence removed from Redis.", userId);
                }
            }

            await next(context, exception);
        }
    }
}
